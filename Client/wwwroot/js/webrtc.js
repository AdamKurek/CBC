let localStream;
let peerConnection;
let offer;
let answer;
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
        peerConnection = new RTCPeerConnection(configuration);

        peerConnection.onicecandidate = event => {
            if (event.candidate) {
                const iceCandidate = event.candidate;
                // Send the iceCandidate to the remote peer using your signaling server
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
            console.error('No local stream available');
        }
    } catch (error) {
        console.error('Failed to create peer connection:', error);
    }
}

async function createOffer() {
    try {
        offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        // Send the offer to the remote peer using your signaling server
    } catch (error) {
        console.error('Error creating offer:', error);
    }
}
async function offerfn() {
    return JSON.stringify(offer.sdp);
}

async function answerfn() {
    return JSON.stringify(answer.sdp);
}
async function createAnswer() {
    try {
        answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        console.log(answer);
    } catch (error) {
        console.error('Error creating answer:', error);
    }
}

async function handleRemoteOffer(off) {
    console.log("handling     "+ off);
    const offerxd = new RTCSessionDescription({
        type: "offer",
        sdp: off
    });
    console.log("handlingtotally     " + offerxd.sdp);
    let ans;
    try {
        await peerConnection.setRemoteDescription(offerxd);
        ans = await peerConnection.createAnswer();
    } catch (e) {
        console.log("apa?e"+e);
    }
    console.log("handling     " + ans);
    return JSON.stringify(ans.sdp);
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
