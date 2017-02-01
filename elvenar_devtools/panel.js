function copyToClipboard(text) {
    if (window.clipboardData && window.clipboardData.setData) {
        // IE specific code path to prevent textarea being shown while dialog is visible.
        return clipboardData.setData("Text", text);

    } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
        var textarea = document.createElement("textarea");
        textarea.textContent = text;
        textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in MS Edge.
        document.body.appendChild(textarea);
        textarea.select();
        try {
            return document.execCommand("copy");  // Security exception may be thrown by some browsers.
        } catch (ex) {
            console.warn("Copy to clipboard failed.", ex);
            return false;
        } finally {
            document.body.removeChild(textarea);
        }
    }
}

function copyTextArea(id) {
    var text_val = document.getElementById(id).value;
    copyToClipboard(text_val);
}

document.querySelector('#copy_guild').addEventListener('click', function () {
    copyTextArea("guild_text");
}, false);

document.querySelector('#copy_tournament').addEventListener('click', function () {
    copyTextArea("tournament_text");
}, false);

document.querySelector('#copy_citymap').addEventListener('click', function () {
    copyTextArea("citymap_text");
}, false);

document.querySelector('#copy_ranking').addEventListener('click', function () {
    copyTextArea("ranking_text");
}, false);

document.querySelector('#copy_cityentities').addEventListener('click', function () {
    copyTextArea("cityentities_text");
}, false);

document.querySelector('#copy_research').addEventListener('click', function () {
    copyTextArea("research_text");
}, false);
