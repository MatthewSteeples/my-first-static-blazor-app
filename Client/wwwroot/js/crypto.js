// WebCrypto API functions for browser identity
window.generateKeyPair = async function() {
    try {
        // Check if WebCrypto is available
        if (!window.crypto || !window.crypto.subtle) {
            throw new Error('WebCrypto API not available');
        }

        // Generate ECDSA key pair with P-256 curve
        const keyPair = await window.crypto.subtle.generateKey(
            {
                name: "ECDSA",
                namedCurve: "P-256"
            },
            true, // extractable
            ["sign", "verify"]
        );

        // Export public key as JWK
        const publicKeyJwk = await window.crypto.subtle.exportKey("jwk", keyPair.publicKey);
        
        // Export private key as JWK
        const privateKeyJwk = await window.crypto.subtle.exportKey("jwk", keyPair.privateKey);

        return {
            publicKey: JSON.stringify(publicKeyJwk),
            privateKey: JSON.stringify(privateKeyJwk)
        };
    } catch (error) {
        console.error('Failed to generate key pair:', error);
        throw error;
    }
};