const { FaceMesh } = require("../../../../../../node_modules/@mediapipe/face_mesh/index");

function ff() {
    console.log("prubuje");
    const video = document.getElementById('localVideo');
    const rpt = document.getElementById('report-button');
    setInterval(async () => {
        const detections = await faceapi.detectAllFaces(video, new faceapi.TinyFaceDetectorOptions())
            //.withFaceLandmarks()
            //.withFaceExpressions()
            .withAgeAndGender();
        console.log("jjj" + detections + " xd");
    }, 100);


    window.faceDetectionInterop = {
        startFaceDetection: async () => {
            console.log("uno");

            try {
                const MODEL_URL = '/weights';
                await faceapi.loadTinyFaceDetectorModel(MODEL_URL);
                //await faceapi.loadFaceLandmarkModel(MODEL_URL);
                //await faceapi.loadFaceExpressionModel(MODEL_URL);
                await faceapi.loadAgeGenderModel(MODEL_URL);
                console.log("zalalldowne");

            } catch (e) {
                console.log("ddd" + e+"\n\n");
            }
        }
    };

    window.faceDetectionInterop.startFaceDetection();
    console.log("tiro");

}