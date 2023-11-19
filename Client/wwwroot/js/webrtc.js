let localStream;
let peerConnection;
let offer;
let answer;
let blackStream; 
let hubconnection;

const configuration = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }
    // ,{ urls: 'turn:your-turn-server.com', username: 'your-username', credential: 'your-password' }
    ]
};

async function getLocalStream() {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        const localVideo = document.getElementById('localVideo');
        localVideo.srcObject = localStream;
    } catch (error) {
        console.error('Error getting user media:', error);
    }
}



async function createPeerConnection() {
    try {
        console.log()
        peerConnection = new RTCPeerConnection(configuration);

        peerConnection.onicecandidate = event => {
            if (event.candidate) {
                const iceCandidate = event.candidate; 
                DotNet.invokeMethodAsync("CBC.Client", "CSendIceCandidate", iceCandidate);
            }
        };

        peerConnection.oniceconnectionstatechange = () => {
            switch (peerConnection.iceConnectionState) {
                case 'checking':
                    console.log('Checking ICE connection');
                    break;
                case 'connected':
                    console.log('ICE connection established');
                    break;
                case 'completed':
                    console.log('ICE connection completed');
                    break;
                case 'disconnected':
                    console.log('ICE connection disconnected');
                    break;
                case 'failed':
                    console.log('ICE connection failed');
                    break;
                case 'closed':
                    console.log('ICE connection closed');
                    break;
                default:
                    console.log('Unknown ICE connection state');
            }
        };



        peerConnection.ontrack = event => {
            const remoteVideo = document.getElementById('remoteVideo');
            remoteVideo.srcObject = event.streams[0];
        };

        if (localStream) {
            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
            });
        } else {
            const blackVideo = document.createElement('video');
            blackVideo.style.display = 'none';
            blackVideo.src = 'data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMjAwIiBoZWlnaHQ9IjEwMCIgZmlsbD0iI2VlZSIvPjwvc3ZnPgo=';
            blackVideo.loop = true;
            let stream = blackVideo.captureStream();
            stream.getTracks().forEach(track => {
                peerConnection.addTrack(track, stream);
            });
        }


    } catch (error) {
        console.error('Failed to create peer connection:', error);
    }
}


async function offerfn() {
    //return peerConnection.localDescription.sdp;
    return offer;
}

async function answerfn() {
    return answer;
}
//async function createAnswer() {
//    try {
//        answer = await peerConnection.createAnswer();
//        await peerConnection.setLocalDescription(answer);
//        await peerConnection.setRemoteDescription(answer);
//        console.log(answer);
//    } catch (error) {
//        console.error('Error creating answer:', error);
//    }
//}
async function createOffer() {
    try {
        offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        //console.log("?e to????    " + peerConnection.localDescription.sdp);
        // Send the offer to the remote peer using your signaling server
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}
async function handleRemoteOffer(off) {
    try {
        await peerConnection.setRemoteDescription(off);
        answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
    } catch (e) {
        console.log("apa?e"+e);
    }
    console.log(answer);
    return answer;
}

async function handleRemoteOfferAndConnect(ansss) {
    try {
        await peerConnection.setRemoteDescription(ansss);
        
        console.log("pooczyloe    " + peerConnection.connectionState + "   spacae      "+ peerConnection.iceConnectionState);
    } catch (e) {
        console.log("apa?e" + e);
    }
}


async function handleRemoteAnswer(answer) {
    await peerConnection.setRemoteDescription(answer);
}

async function receiveIceCandidate(iceCandidateJson) {
    console.log("recivededded " + iceCandidateJson);

    try {
        let iceCandidateInit = JSON.parse(iceCandidateJson);
        var iceCandidate = new RTCIceCandidate(iceCandidateInit);
        peerConnection.addIceCandidate(iceCandidate);
    } catch (error) {
        console.error(error);
    }
}

async function closePeerConnection() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
}
