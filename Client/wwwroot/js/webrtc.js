let localStream;
let remoteVideo;
let peerConnection;
let offer;
let answer;
let blackStream; 
let hubconnection;
let MessagesDataChannel;
//const lclvideo = document.getElementById('localVideo');//dumbo

window.onerror = function (message, source, lineno, colno, error) {
    alert("Uncaught exception:\n" + message + "\nSource: " + source + "\nLine: " + lineno + "\nColumn: " + colno);
    console.error(error);
    return true;
};


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
            console.log('LocalSteam is: ', localStream);
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

        MessagesDataChannel = peerConnection.createDataChannel("ChatChannel");
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
        console.log("failed to handleRemoteOffer: "+e);
    }
    console.log(answer);
    return answer;
}

async function handleRemoteOfferAndConnect(ansss) {
    try {
        await peerConnection.setRemoteDescription(ansss);
       // console.log("pooczyloe    " + peerConnection.connectionState + "   spacae      "+ peerConnection.iceConnectionState);
    } catch (e) {
        console.log("failed to setRemoteDescription: " + e);
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

async function disconnectCall(connId) {
    console.log("bylo called disconnect");
    try {
        if (!peerConnection) {
            console.log("nie peerdol");
            return;
        }
        const wrapper = document.getElementById('recent-matches');

        var childCount = wrapper.children.length;
        for (var i = 0; i < childCount; i++) {
            var child = wrapper.children[i];
            if (child.id === connId) {
                wrapper.prepend(child);
                disconnect();
                return;
            }
        }
        
        var canvas = document.createElement('canvas');
        canvas.className = 'clickable-canvas';
        //canvas.addEventListener('click', function () {
        //    alert(canvas.id.toString());
        //});

        var buttonTemplate = document.getElementById('buttonTemplate');
        var clonedButtons = buttonTemplate.content.cloneNode(true);
        var DivWith2Divs = clonedButtons.querySelector('div');

       
        DivWith2Divs.id = connId;


        var DivWithCallButtons = DivWith2Divs.querySelector('div');
        var DivWithReceiveCallButtons = DivWith2Divs.querySelector('div:nth-child(2)');

        var clonedButtonsButtons = DivWithCallButtons.querySelectorAll('button');

        { 
            var i = 0;
            clonedButtonsButtons.forEach(function (button) {
               // button.
            });
        }

        //clonedButtonDiv.children[0].classList.add('callButton'); // clown statement
        DivWithCallButtons.children[0].addEventListener('click', function () {
            callClicked(connId);
        });

        DivWithCallButtons.children[1].addEventListener('click', function () {
            DivWith2Divs.parentElement.removeChild(DivWith2Divs);
        });

        DivWithCallButtons.children[2].addEventListener('click', function () {

        });


        DivWithReceiveCallButtons.children[0].addEventListener('click', function () {
            callAccept(connId);
            DivWithReceiveCallButtons.style.visibility = 'hidden';
        });

        DivWithReceiveCallButtons.children[1].addEventListener('click', function () {
            callDeny(connId);
            DivWithReceiveCallButtons.style.visibility = 'hidden';
        });


        DivWithReceiveCallButtons.style.visibility = 'hidden';//todo it has to be here to store calling state, it probably should be done some other way
        DivWith2Divs.addEventListener('mouseenter', function () {
            if (DivWithReceiveCallButtons.style.visibility === 'hidden')
                DivWithCallButtons.style.visibility = 'visible';
        });

        DivWith2Divs.addEventListener('mouseleave', function () {
            DivWithCallButtons.style.visibility = 'hidden';
        });

        canvas.style.width = '100%';
        canvas.style.height = '100%';
        canvas.style.zIndex = -1;
        canvas.style.position = 'absolute';

        DivWithCallButtons.style.height = "75%";
        DivWithCallButtons.style.width= "100%";
        DivWithCallButtons.style.position = 'absolute';
        DivWithCallButtons.style.backgroundColour = "yellow";
        DivWithCallButtons.style.alignItems = 'end';
        DivWithCallButtons.style.justifyContent = 'center';

        DivWith2Divs.appendChild(canvas);


        const ctx = canvas.getContext('2d');
        wrapper.insertBefore(DivWith2Divs, wrapper.firstChild);

        if (remoteVideo) {
            if (remoteVideo.videoWidth > 0) {
                console.log("wszed to rysowanie z filmu");

                remoteVideo.pause();
                canvas.width = remoteVideo.videoWidth;
                canvas.height = remoteVideo.videoHeight;
                ctx.drawImage(remoteVideo, 0, 0, remoteVideo.videoWidth, remoteVideo.videoHeight);
                return;
            }
        }
        const img = new Image();
        img.src = "images/user-icon.png";
        img.onload = function () {
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;
            ctx.drawImage(img, 0, 0);
            img.onerror = function () {
                console.error("Error loading the image." + img);
            };
        }
    } catch (e) {
        console.error("canmvas error" + e);
    } finally {
        disconnect();
    }
   
}

function disconnect() {

    localStream.getTracks().forEach(track => {//there are fancy ways
        track.stop();
    });
    if (MessagesDataChannel) {
        MessagesDataChannel.close();
    }
    if (peerConnection) {
        peerConnection.close();
        delete peerConnection;
    }
}

function createCanvas() {
   
    return canvas;
}

function callClicked(Id) {
    DotNet.invokeMethodAsync("CBC.Client", "Call",Id);
}

function Calling(who) {
    const canvases = document.getElementById('recent-matches');
    const targetElement = canvases.querySelector(`#${who}`);
    
    if (targetElement) {
        console.log('Found element:', targetElement);
        targetElement.querySelector('div').style.visibility = 'hidden';
        var receiveCallButtons = targetElement.querySelector('div:nth-child(2)');
        receiveCallButtons.style.visibility = 'visible';
    } else {
        console.log('Element not found');
    }
}

function callAccept(connId){
    DotNet.invokeMethodAsync("CBC.Client", "AcceptCall", connId);
}

function callDeny(connId){
    DotNet.invokeMethodAsync("CBC.Client", "DenyCall", connId);
}

function CallDenied(who) {
    const canvases = document.getElementById('recent-matches');
    const targetElement = canvases.querySelector(`#${who}`);

    if (targetElement) {
       // console.log('Found element:', targetElement);
        //targetElement.querySelector('div').style.visibility = 'hidden';
        var receiveCallButtons = targetElement.querySelector('div:nth-child(2)');
        receiveCallButtons.style.visibility = 'hidden';
    } else {
        console.log('Denied not found');
    }
}