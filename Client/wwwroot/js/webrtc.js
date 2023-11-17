let localStream;
let peerConnection;
const configuration = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };


console.log("Script executed");
async function getLocalStream() {
    console.log("getLocalStream xd");

    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        const localVideo = document.getElementById('localVideo');
        localVideo.srcObject = localStream;
    } catch (error) {
        console.error('Error getting user media:', error);
    }
}
async function printCringe() {
    console.log("cringe");
}

function createPeerConnection() {
    peerConnection = new RTCPeerConnection(configuration);

    // Handle ICE candidate generation
    peerConnection.onicecandidate = event => {
        if (event.candidate) {
            // Send the ICE candidate to the other peer
            const iceCandidate = event.candidate;
            sendIceCandidateToOtherPeer(iceCandidate);
        }
    };

    // Handle incoming video track
    peerConnection.ontrack = event => {
        const remoteVideo = document.getElementById('remoteVideo');
        remoteVideo.srcObject = event.streams[0];
    };

    // Add local tracks to the peer connection
    localStream.getTracks().forEach(track => {
        peerConnection.addTrack(track, localStream);
    });
}

function sendIceCandidateToOtherPeer(iceCandidate) {
    hubConnection.invoke("SendIceCandidate", iceCandidate);
    console.log("hubConnection xd");
}

function sendIceCandidate(candidate) {
    // Assuming you have a SignalR hub connection named "hubConnection"
    hubConnection.invoke("SendIceCandidate", targetConnectionId, candidate)
        .catch(error => console.error('Error sending ice candidate:', error));
}

async function createOffer() {
    try {
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}

async function handleRemoteOffer(offer) {
    await peerConnection.setRemoteDescription(offer);
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);
}

async function handleRemoteAnswer(answer) {
    await peerConnection.setRemoteDescription(answer);
}

function handleRemoteIceCandidate(candidate) {
    peerConnection.addIceCandidate(candidate);
}

function closePeerConnection() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
}
