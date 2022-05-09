
/* Copyright 2020 Google LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


// Device Access Variables:
let streamExtensionToken = "";


/** deviceAccessRequest - Issues requests to Device Access Rest API */
function deviceAccessRequest(method, call, localpath, payload = "") {
  let xhr = new XMLHttpRequest();
  xhr.open(method, selectedAPI + localpath);
  xhr.setRequestHeader('Authorization', 'Bearer ' + accessToken);
  xhr.setRequestHeader('Content-Type', 'application/json; charset=UTF-8');

  xhr.onload = function () {
    if(xhr.status === 200) {
      let responsePayload = "* Payload: \n" + xhr.response;
      //pushLog(LogType.HTTP, method + " Response", responsePayload);
      deviceAccessResponse(method, call, xhr.response);
    } else {
      //pushError(LogType.HTTP, method + " Response", xhr.responseText);
    }
  };

  let requestEndpoint = "* Endpoint: \n" + selectedAPI + localpath;
  let requestAuthorization = "* Authorization: \n" + 'Bearer ' + accessToken;
  let requestPayload = "* Payload: \n" + JSON.stringify(payload, null, 4);
  //pushLog(LogType.HTTP, method + " Request",      requestEndpoint + "\n\n" + requestAuthorization + "\n\n" + requestPayload);

  if (method === 'POST' && payload && payload !== "") {
    xhr.send(JSON.stringify(payload));
  } else {
    xhr.send();
  }
}


/** deviceAccessResponse - Parses responses from Device Access API calls */
function deviceAccessResponse(method, call, response) {
  //console.log(LogType.HTTP, method + " Response", response);
  let data = JSON.parse(response);
  // Check if response data is empty:
  if(!data) {
    pushError(LogType.ACTION, "Empty Response!", "Device Access response contains empty response!");
    return;
  }
  // Based on the original request call, interpret the response:
  switch(call) {  
	
    case 'generateStream':
      console.log("Generate Stream!");
      if(data["results"] && data["results"].hasOwnProperty("mediaSessionId"))
        streamExtensionToken = data["results"].mediaSessionId;
      if(data["results"] && data["results"].hasOwnProperty("answerSdp")) {
        updateWebRTC(data["results"].answerSdp);
        console.log("[Video Stream] mediaId: " + data["results"].mediaSessionId);
      }
      break;    
    case 'stopStream':
      console.log("Stop Stream!");
      
      break;
    
    default:
      pushError(LogType.ACTION, "Error", "Unrecognized Request Call!");
  }
}



/// Device Access API ///

/** onListDevices - Issues a ListDevices request */
function onListDevices() {
  let endpoint = "/enterprises/" + projectId + "/devices";
  deviceAccessRequest('GET', 'listDevices', endpoint);
}





/** onGenerateStream_WebRTC - Issues a GenerateWebRtcStream request */
function onGenerateStream_WebRTC(deviceId, offerSDP) {
  projectId = document.getElementById('projectId').value;
  accessToken = document.getElementById('secret').value;


  let endpoint = "/enterprises/" + projectId + "/devices/" + deviceId + ":executeCommand";
  let payload = {
    "command": "sdm.devices.commands.CameraLiveStream.GenerateWebRtcStream",
    "params": {
      "offer_sdp": offerSDP
    }
  };

  deviceAccessRequest('POST', 'generateStream', endpoint, payload);
}



/** onStopStream_WebRTC - Issues a StopWebRtcStream request */
function onStopStream_WebRTC(deviceId) {
  let endpoint = "/enterprises/" + projectId + "/devices/" + deviceId + ":executeCommand";
  let payload = {
    "command": "sdm.devices.commands.CameraLiveStream.StopWebRtcStream",
    "params": {
      "mediaSessionId" : streamExtensionToken
    }
  };
  deviceAccessRequest('POST', 'stopStream', endpoint, payload);
}
