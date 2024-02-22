//const { FaceMesh } = require("../../../../../../node_modules/@mediapipe/face_mesh/index");

function ff() {
    const video = document.getElementById('localVideo');
   
    //  [{"detection":{"_imageDims":{"_width":640,"_height":480},"_score":0.7914975366296669,"_classScore":0.7914975366296669,"_className":"","_box":{"_x":228.22706625217947,"_y":176.37306151025538,"_width":169.24086190395556,"_height":168.32601212149473}},"gender":"male","genderProbability":0.8330137133598328,"age":19.217952728271484}]

    window.faceDetectionInterop = {
        startFaceDetection: async () => {

            try {
                const MODEL_URL = '/weights';
                await faceapi.loadTinyFaceDetectorModel(MODEL_URL);
                await faceapi.loadFaceLandmarkModel(MODEL_URL);
                await faceapi.loadFaceExpressionModel(MODEL_URL);
                await faceapi.loadAgeGenderModel(MODEL_URL);

            } catch (e) {
                console.log("Failed loading face models" + e+"\n\n");
            }
        }
    };

    window.faceDetectionInterop.startFaceDetection();
    console.log("tiro");
    setInterval(async () => {
        try {
            const detections = await faceapi.detectAllFaces(video, new faceapi.TinyFaceDetectorOptions())
                //.withFaceLandmarks()
                //.withFaceExpressions()
                .withAgeAndGender();
            console.log(detections[0].age);
            DotNet.invokeMethodAsync("CBC.Client", "SetAge", detections[0].age);
            DotNet.invokeMethodAsync("CBC.Client", "SetIsFemale", detections[0].gender === 'female' ? detections[0].genderProbability : 1 - detections[0].genderProbability);
        } catch (e) {
            console.log("Face detection failed" + e + "\n\n");
        }
    }, 1000);
}