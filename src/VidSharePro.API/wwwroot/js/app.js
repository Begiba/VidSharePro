const API_URL = 'http://localhost:5020/api';

$(document).ready(function () {
    checkAuth();

    // --- UI Logic ---
    $('#btn-show-upload').click(() => $('#modal-upload').fadeIn());
    $('.btn-close').click(() => $('#modal-upload').fadeOut());
    $('#btn-back').click(() => {
        $('#sec-player').hide();
        $('#sec-dashboard').show();
        $('#main-video-player')[0].pause();
    });

    // --- Authentication ---
    $('#form-login').submit(function (e) {
        e.preventDefault();
        const data = {
            email: $('#login-email').val(),
            password: $('#login-pass').val()
        };

        $.ajax({
            url: `${API_URL}/auth/login`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: (res) => {
                localStorage.setItem('jwt_token', res.token);
                localStorage.setItem('username', res.username);
                checkAuth();
            },
            error: () => alert('Invalid credentials')
        });
    });

    $('#btn-logout').click(() => {
        localStorage.clear();
        location.reload();
    });

    // Delegate click event for dynamically created play buttons
    $('#video-list').on('click', '.btn-play', function () {
        // Pull the ID and Title from the data attributes we created in Step 1
        const id = $(this).data('id');
        const title = $(this).data('title');

        // Call your existing playVideo function
        playVideo(id, title);
    });

    // --- Video Upload (Streaming) ---
    $('#btn-do-upload').click(function () {
        const file = $('#up-file')[0].files[0];
        const title = $('#up-title').val();

        if (!file || !title) return alert("Please provide a title and a file.");

        const formData = new FormData();
        formData.append('file', file);
        formData.append('Title', title);
        
        $('.progress-wrapper').show();
        $('#progress-bar').css('width', '0%');
        $('#progress-percent').text('0%');

        $.ajax({
            url: `${API_URL}/videos/upload`,
            type: 'POST',
            headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` },
            data: formData,
            processData: false, // Required for file upload
            contentType: false, // Required for file upload
            xhr: function () {
                const xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        const percentComplete = Math.round((evt.loaded / evt.total) * 100);
                        $('#progress-bar').css('width', percentComplete + '%');
                        $('#progress-percent').text(percentComplete + '%');
                        if (percentComplete === 100) {
                            $('#progress-percent').text('Processing on server...');
                        }
                    }
                }, false);
                return xhr;
            },
            success: () => {
                alert('Upload successful! Processing started.');
                $('#modal-upload').fadeOut();
                $('.progress-wrapper').hide();
                loadVideos();
            },
            error: function (err) {
                alert('Upload failed. Ensure the file is under the allowed limit.');
                alert(JSON.stringify(err));
                $('.progress-wrapper').hide();
            }
        });
    });
});

function checkAuth() {
    const token = localStorage.getItem('jwt_token');
    if (token) {
        $('#sec-login').hide();
        $('#sec-dashboard').show();
        $('#nav-user').show();
        $('#username-display').text(localStorage.getItem('username'));
        loadVideos();
    } else {
        $('#sec-login').show();
        $('#sec-dashboard').hide();
        $('#nav-user').hide();
    }
}

function loadVideos() {
    $.ajax({
        url: `${API_URL}/videos`,
        type: 'GET',
        headers: { 'Authorization': `Bearer ${localStorage.getItem('jwt_token')}` },
        success: (videos) => {

            const grid = $('#video-list');
            grid.empty();
            videos.forEach(v => {
                const status = (v.status !== undefined && v.status !== null)
                    ? String(v.status).toLowerCase()
                    : 'unknown';
                const thumbUrl = `${API_URL}/thumbnails/${v.id}`;
                const isReady = v.status === 'Ready'; // Assuming 'Ready' is your Enum string
                // REMOVED onclick, ADDED class "btn-play" and data attributes
                const playButtonHtml = isReady
                    ? `<button class="btn-play" data-id="${v.id}" data-title="${v.title}">Play</button>`
                    : `<button disabled class="btn-processing">Processing...</button>`;
                grid.append(`
                    <div class="video-card">
                        <div class="thumb-container">
                            <img src="${thumbUrl}" alt="Preview" onerror="this.src='img/processing.png'">
                        </div>
                        <h4>${v.title}</h4>
                        <p class="status-${status}">${v.status}</p>
                        ${playButtonHtml}
                    </div>
                `);
            });
        }
    });
}

function playVideo(id, title) {
    const token = localStorage.getItem('jwt_token');
    const streamUrl = `${API_URL}/videos/${id}/stream?token=${token}`; // Append token here

    $('#sec-dashboard').hide();
    $('#sec-player').fadeIn();
    $('#playing-title').text(title);

    const videoPlayer = $('#main-video-player')[0];
    // 1. Update the source
    videoPlayer.src = streamUrl;

    // 2. Load the metadata and buffer
    videoPlayer.load();

    // 3. Handle the play promise to prevent the AbortError
    const playPromise = videoPlayer.play();

    if (playPromise !== undefined) {
        playPromise.then(_ => {
            // Automatic playback started!
            console.log("Playback started successfully.");
        }).catch(error => {
            // Auto-play was prevented or interrupted
            console.error("Playback failed or was interrupted:", error);
        });
    }
}