let localStream;
let remoteVideo;
let peerConnection;
let offer;
let answer;
let blackStream; 
let hubconnection;
let MessagesDataChannel;
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

    if (constraints.video) {
        const videoTrack = emptyStream.getVideoTracks()[0];
        emptyStream.addTrack(videoTrack);
    }

    if (constraints.audio) {
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
        peerConnection = new RTCPeerConnection(configuration);

        MessagesDataChannel = peerConnection.createDataChannel("MyChatChannel");
        MessagesDataChannel.onmessage = (event) => {
            //console.log("jedynka + "+event.data);
            DotNet.invokeMethodAsync("CBC.Client", "GotMessage", event.data);
        };
        peerConnection.ondatachannel = event => {
            MessagesDataChannel = event.channel;
            MessagesDataChannel.onmessage = (event) => {
                //console.log("dwujka + "+event);
                DotNet.invokeMethodAsync("CBC.Client", "GotMessage", event.data);
            };
        };

        peerConnection.onicecandidate = event => {
            if (event.candidate) {
                const iceCandidate = event.candidate; 
                DotNet.invokeMethodAsync("CBC.Client", "CSendIceCandidate", iceCandidate);
            } else {
                console.log('All ICE candidates have been gathered');
            }
        };
        peerConnection.ontrack = event => {
            remoteVideo = document.getElementById('remoteVideo');
            if (event.streams.length > 0) {
                remoteVideo.srcObject = event.streams[0];
            } else {
            }
        };

        peerConnection.addEventListener('iceconnectionstatechange', function () {
            switch (peerConnection.iceConnectionState) {
                case 'disconnected':
                case 'failed':
                case 'closed':
                    DotNet.invokeMethodAsync("CBC.Client", "StartReconnecting");
                    break;
            }
        });

        if (localStream) {
            localStream.getTracks().forEach(track => {
                peerConnection.addTrack(track, localStream);
            });
        }
    } catch (error) {
        console.error('Failed to create peer connection:', error);
    }
}

async function sendChat(text) {
    console.log("wysylam " + text);
    try {
        MessagesDataChannel.send(text);
    }
    catch(e) {
        console.error('Chat not sent:', e);
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
       // console.log("pooczyloe    " + peerConnection.connectionState + "   spacae      "+ peerConnection.iceConnectionState);
    } catch (e) {
        console.log("apa?e" + e);
    }
}

async function handleRemoteAnswer(answer) {
    await peerConnection.setRemoteDescription(answer);
}

async function receiveIceCandidate(iceCandidateJson) {
    //console.log("recivededded ICE");
    try {
        let iceCandidateInit = JSON.parse(iceCandidateJson);
        var iceCandidate = new RTCIceCandidate(iceCandidateInit);
        peerConnection.addIceCandidate(iceCandidate);
    } catch (error) {
        console.error("ICE error: " + error);
    }
}

async function closePeerConnectionO() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
}


async function closePeerConnection() {
    console.log("bylo called");

    try {
        if (!peerConnection) {
            console.log("nie peerdol");
            return;
        }

        if (remoteVideo) {
            const canvas = createCanvas();
            const ctx = canvas.getContext('2d');
            const wrapper = document.getElementById('recent-matches');
            remoteVideo.pause();
            canvas.width = remoteVideo.videoWidth;
            canvas.height = remoteVideo.videoHeight;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(remoteVideo, 0, 0);
            wrapper.insertBefore(canvas, wrapper.firstChild);

            console.log("canvas is:" + canvas);
            const dataURL = canvas.toDataURL('image/png');
            if (peerConnection) {
                peerConnection.close();
                peerConnection = null;
            }

            

            var imageData = ctx.getImageData(0, 0, canvas.width, canvas.height).data;

            // Check if all pixels are white
            var isAllWhite = true;
            for (var i = 0; i < imageData.length; i += 4) {
                if (imageData[i] !== 255 || imageData[i + 1] !== 255 || imageData[i + 2] !== 255) {
                    isAllWhite = false;
                    console.log( "za +"+imageData[i+1]);
                    break;
                }
            }

            if (isAllWhite) {
                console.log('Image is all white.');
            } else {
                console.log('Image has content. ' + imageData.length);
            }
        } else {
            console.error('No video stream available');
        }

    } catch (e) {
        console.error("canmvas error" + e);
    }
}

function createCanvas() {
    var canvas = document.createElement('canvas');
    canvas.className = 'clickable-canvas';
    canvas.width = 300;
    canvas.height = 200;
    canvas.addEventListener('click', function () {
        alert('Canvas Clicked!');
    });

    return canvas;
}