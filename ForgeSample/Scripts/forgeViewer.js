var viewerApp;

//Fonction pour afficher le modèle 3D 

function showModel(urn) {
    var options = {
        env: 'AutodeskProduction',
        getAccessToken: getAccessToken,
        refreshToken: getAccessToken
    };
            //
            // TODO: Replace static access token string below with call to fetch new token from your backend
            // Both values are provided by Forge's Authentication (OAuth) API.
            //
            // Example Forge's Authentication (OAuth) API return value:
            // {
            //    "access_token": "<YOUR_APPLICATION_TOKEN>",
            //    "token_type": "Bearer",
            //    "expires_in": 86400
            // }
            //
    //        var accessToken = getAccessToken;
    //        var expireTimeSeconds = 60 * 30;
    //        onGetAccessToken(accessToken, expireTimeSeconds);
        

    //};
    var documentId = 'urn:' + urn;
    Autodesk.Viewing.Initializer(options, function onInitialized() {
        viewerApp = new Autodesk.Viewing.ViewingApplication('forgeViewer');
        viewerApp.registerViewer(viewerApp.k3D, Autodesk.Viewing.Private.GuiViewer3D);
        viewerApp.loadDocument(documentId, onDocumentLoadSuccess, onDocumentLoadFailure);
    });
}

// Fonction Api pour récupérer le Token 
function getAccessToken() {
    var xmlHttp = null;
    xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", '/api/forge/token', false /*forge viewer requires SYNC*/);
    xmlHttp.send(null);
    return xmlHttp.responseText;
}

function onDocumentLoadSuccess(doc) {

    // We could still make use of Document.getSubItemsWithProperties()
    // However, when using a ViewingApplication, we have access to the **bubble** attribute,
    // which references the root node of a graph that wraps each object from the Manifest JSON.
    var viewables = viewerApp.bubble.search({ 'type': 'geometry' });
    if (viewables.length === 0) {
        console.error('Document contains no viewables.');
        return;
    }

    // Choose any of the avialble viewables
    viewerApp.selectItem(viewables[0].data, onItemLoadSuccess, onItemLoadFail);
}

function onDocumentLoadFailure(viewerErrorCode) {
    console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
}

function onItemLoadSuccess(viewer, item) {
    console.log('onItemLoadSuccess()!');
    console.log(viewer);
    console.log(item);

    // Congratulations! The viewer is now ready to be used.
    console.log('Viewers are equal: ' + (viewer === viewerApp.getCurrentViewer()));
}

function onItemLoadFail(errorCode) {
    console.error('onItemLoadFail() - errorCode:' + errorCode);
}
