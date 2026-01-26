// Periodic Background Sync bootstrap + JWT creation (ES256) using the browser identity keys.
// Stores the JWT in IndexedDB so the service worker can use it when running without an open tab.

(function () {
	const CONSENT_KEY = 'periodic-background-sync:consent:v1';
	const IDENTITY_STORAGE_KEY = 'BrowserIdentity';
	const DB_NAME = 'BlazorTracker';
	const DB_VERSION = 1;
	const STORE_NAME = 'auth';
	const JWT_KEY = 'jwt';

	function base64UrlEncodeBytes(bytes) {
		let binary = '';
		for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i]);
		const b64 = btoa(binary);
		return b64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
	}

	function base64UrlEncodeJson(obj) {
		const json = JSON.stringify(obj);
		const bytes = new TextEncoder().encode(json);
		return base64UrlEncodeBytes(bytes);
	}

	function openDb() {
		return new Promise((resolve, reject) => {
			const request = indexedDB.open(DB_NAME, DB_VERSION);
			request.onupgradeneeded = () => {
				const db = request.result;
				if (!db.objectStoreNames.contains(STORE_NAME)) {
					db.createObjectStore(STORE_NAME);
				}
			};
			request.onsuccess = () => resolve(request.result);
			request.onerror = () => reject(request.error);
		});
	}

	async function idbSet(key, value) {
		const db = await openDb();
		return new Promise((resolve, reject) => {
			const tx = db.transaction(STORE_NAME, 'readwrite');
			tx.oncomplete = () => resolve();
			tx.onerror = () => reject(tx.error);
			tx.objectStore(STORE_NAME).put(value, key);
		});
	}

	function tryParseJson(value) {
		try {
			return JSON.parse(value);
		} catch {
			return undefined;
		}
	}

	async function sha256Base64Url(inputBytes) {
		const digest = new Uint8Array(await crypto.subtle.digest('SHA-256', inputBytes));
		return base64UrlEncodeBytes(digest);
	}

	async function getEs256PublicJwkThumbprint(publicKeyJwk) {
		// RFC 7638 JWK Thumbprint, specialized for EC P-256 public keys.
		if (!publicKeyJwk || typeof publicKeyJwk !== 'object') return undefined;
		if (publicKeyJwk.kty !== 'EC' || publicKeyJwk.crv !== 'P-256') return undefined;
		if (typeof publicKeyJwk.x !== 'string' || typeof publicKeyJwk.y !== 'string') return undefined;

		// Canonical JSON members in lexicographic order: crv, kty, x, y
		const canonical = {
			crv: publicKeyJwk.crv,
			kty: publicKeyJwk.kty,
			x: publicKeyJwk.x,
			y: publicKeyJwk.y
		};

		const json = JSON.stringify(canonical);
		const bytes = new TextEncoder().encode(json);
		return sha256Base64Url(bytes);
	}

	async function createEs256JwtFromBrowserIdentity() {
		const identityRaw = window.localStorage?.getItem(IDENTITY_STORAGE_KEY);
		if (!identityRaw) return undefined;

		const identity = tryParseJson(identityRaw);
		if (!identity || typeof identity !== 'object') return undefined;

		const publicKeyJwk = typeof identity.PublicKey === 'string' ? tryParseJson(identity.PublicKey) : undefined;
		const privateKeyJwk = typeof identity.PrivateKey === 'string' ? tryParseJson(identity.PrivateKey) : undefined;
		if (!publicKeyJwk || !privateKeyJwk) {
			// Likely the WebCrypto fallback identity was used.
			return undefined;
		}

		const publicKeyThumbprint = await getEs256PublicJwkThumbprint(publicKeyJwk);
		if (!publicKeyThumbprint) return undefined;

		const header = {
			alg: 'ES256',
			typ: 'JWT',
			kid: publicKeyThumbprint,
			jwk: publicKeyJwk
		};

		const nowSeconds = Math.floor(Date.now() / 1000);
		const payload = {
			sub: publicKeyThumbprint,
			iat: nowSeconds,
			// Server currently validates signature only, but we include an exp for future-proofing.
			exp: nowSeconds + 60 * 60 * 24 * 30
		};

		const encodedHeader = base64UrlEncodeJson(header);
		const encodedPayload = base64UrlEncodeJson(payload);
		const signingInput = `${encodedHeader}.${encodedPayload}`;
		const data = new TextEncoder().encode(signingInput);

		const key = await crypto.subtle.importKey(
			'jwk',
			privateKeyJwk,
			{ name: 'ECDSA', namedCurve: 'P-256' },
			false,
			['sign']
		);

		// WebCrypto returns a DER-encoded ECDSA signature; the server currently accepts DER.
		const signature = new Uint8Array(await crypto.subtle.sign({ name: 'ECDSA', hash: 'SHA-256' }, key, data));
		const encodedSignature = base64UrlEncodeBytes(signature);
		return `${encodedHeader}.${encodedPayload}.${encodedSignature}`;
	}

	async function ensureJwtStored() {
		if (!window.crypto?.subtle) return;
		const token = await createEs256JwtFromBrowserIdentity();
		if (!token) return;
		await idbSet(JWT_KEY, token);
	}

	async function shouldEnablePeriodicSync() {
		const existing = window.localStorage?.getItem(CONSENT_KEY);
		if (existing === 'allowed') return true;
		if (existing === 'declined' || existing === 'denied' || existing === 'unsupported') return false;

		const allow = window.confirm(
			'Allow background sync?\n\nThis enables the app to periodically sync in the background.'
		);
		window.localStorage?.setItem(CONSENT_KEY, allow ? 'allowed' : 'declined');
		return allow;
	}

	window.periodicSyncSetup = {
		// Returns a freshly generated ES256 JWT using the browser identity keys.
		// Useful for manual testing (e.g., Test.razor) without relying on the SW/IDB flow.
		getJwt: async function () {
			try {
				const token = await createEs256JwtFromBrowserIdentity();
				if (!token) return null;
				await idbSet(JWT_KEY, token);
				return token;
			} catch {
				return null;
			}
		},

		// Registers periodic background sync and prepares an auth token for the service worker.
		initialize: async function () {
			try {
				if (!('serviceWorker' in navigator)) return;
				const registration = await navigator.serviceWorker.ready;
				if (!registration) return;

				if (!('periodicSync' in registration)) {
					window.localStorage?.setItem(CONSENT_KEY, 'unsupported');
					return;
				}

				// If permissions API supports the name, avoid prompting when already denied.
				if (navigator.permissions?.query) {
					try {
						const status = await navigator.permissions.query({ name: 'periodic-background-sync' });
						if (status?.state === 'denied') {
							window.localStorage?.setItem(CONSENT_KEY, 'denied');
							return;
						}
					} catch {
						// ignore
					}
				}

				const enabled = await shouldEnablePeriodicSync();
				if (!enabled) return;

				// Prepare token the service worker will use.
				await ensureJwtStored();

				try {
					await registration.periodicSync.register('sync', {
						minInterval: 12 * 60 * 60 * 1000
					});
				} catch {
					// Some browsers require the PWA to be installed or have other constraints.
				}
			} catch {
				// No-op
			}
		}
	};
})();
