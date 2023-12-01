let localStream;
let peerConnection;
let offer;
let answer;
let blackStream; 
let hubconnection;
//const lclvideo = document.getElementById('localVideo');//dumbo

const configuration = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }
    // ,{ urls: 'turn:your-turn-server.com', username: 'your-username', credential: 'your-password' }
    ]
};

async function getLocalStream() {
    try {

        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        //const localVideo = document.getElementById('localVideo');
        //localVideo.srcObject = localStream;


        //localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });//tenbylsambylook


        //lclvideo.srcObject = localStream;//dumbo
    } catch (error) {
        console.error('NoMedia;  :', error);
        try {
            localStream = createEmptyStream();
            console.error('LocalSteam is: ', localStream);
        } catch (error) {
            console.error('Cotam:', error);
        }
    }
}

function createEmptyStream() {
    const constraints = { video: true, audio: true };
    const canvas = document.createElement('canvas');
    const emptyStream = canvas.captureStream();

    // Replace the tracks in the emptyStream
    if (constraints.video) {
        const videoTrack = emptyStream.getVideoTracks()[0];
        emptyStream.addTrack(videoTrack);
    }

    if (constraints.audio) {
        // Create an AudioContext and OscillatorNode to generate an empty audio track
        const audioCtx = new AudioContext();
        const oscillator = audioCtx.createOscillator();
        const destination = audioCtx.createMediaStreamDestination();
        oscillator.connect(destination);
        oscillator.start();

        const audioTrack = destination.stream.getAudioTracks()[0];
        emptyStream.addTrack(audioTrack);
    }

    return emptyStream;
}
async function createPeerConnection() {
    try {
        console.log()
        peerConnection = new RTCPeerConnection(configuration);

        peerConnection.onicecandidate = event => {
            if (event.candidate) {
                const iceCandidate = event.candidate; 
                DotNet.invokeMethodAsync("CBC.Client", "CSendIceCandidate", iceCandidate);
            } else {
                console.log('All ICE candidates have been gathered');
            }
        };
        peerConnection.ontrack = event => {
            const remoteVideo = document.getElementById('remoteVideo');
            if (event.streams.length > 0) {
                remoteVideo.srcObject = event.streams[0];
            } else {
                //TODO Display a placeholder image or canvas
            }
        };

        if (localStream) {
            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
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
    console.log("recivededded ICE");
    try {
        let iceCandidateInit = JSON.parse(iceCandidateJson);
        var iceCandidate = new RTCIceCandidate(iceCandidateInit);
        peerConnection.addIceCandidate(iceCandidate);
    } catch (error) {
        console.error("ICE error: " + error);
    }
}

async function closePeerConnection() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
}
