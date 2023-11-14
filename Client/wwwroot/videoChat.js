let localVideo;
let remoteVideo;
let pConnection;
let lclStream;

async function startCall(dotNetObjRef) {
    localVideo = document.getElementById("localVideo");
    remoteVideo = document.getElementById("remoteVideo");

    lclStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
    localVideo.srcObject = lclStream;

    //STUN/TURN server
    const configuration = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };

    pConnection = new RTCPeerConnection(configuration);

    lclStream.getTracks().forEach(track => {
        pConnection.addTrack(track, lclStream);
    });

    pConnection.ontrack = (event) => {
        remoteVideo.srcObject = event.streams[0];
    };

    pConnection.onicecandidate = (event) => {
        if (event.candidate) {
            dotNetObjRef.invokeMethodAsync("OnIceCandidate", JSON.stringify(event.candidate));
        }
    };
}

function closeCall() {
    if (pConnection) {
        pConnection.close();
        pConnection = null;
    }

    if (lclStream) {
        lclStream.getTracks().forEach(track => track.stop());
        lclStream
    }

    localVideo.srcObject = null;
    remoteVideo.srcObject = null;
}



