let localStream;
let peerConnection;
let offer;
let answer;
let blackStream; 


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
async function offerfn() {
    //return peerConnection.localDescription.sdp;
    return offer;
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
    //console.log("handling     "+ off);
    //const offerxd = new RTCSessionDescription({
    //    type: "offer",
    //    sdp: off
    //});
  
    let ans;
    try {
        await peerConnection.setRemoteDescription(off);
        ans = await peerConnection.createAnswer();
    } catch (e) {
        console.log("apa?e"+e);
    }
    console.log("handling     " + ans);
    return JSON.stringify(ans);
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
