document.getElementById('showContactBtn').addEventListener('click', function () {
    this.classList.add('d-none');
    document.getElementById('contactInfo').classList.remove('d-none');
});

function copyFallback(text) {
    const temp = document.createElement('textarea');
    temp.value = text;
    document.body.appendChild(temp);
    temp.select();
    document.execCommand('copy');
    temp.remove();
}

document.querySelectorAll('[data-copy]').forEach(function (btn) {
    btn.addEventListener('click', async function () {
        const text = this.dataset.copy;

        try
        {
            await navigator.clipboard.writeText(text);
        }
        catch 
        {
            copyFallback(text);
        }
        const icon = this.querySelector('i');
        icon.className = 'bi bi-check2';
        this.classList.add('copied');

        setTimeout(() => {
            icon.className = 'bi bi-clipboard';
            this.classList.remove('copied');
        }, 1500);
    });
});

const gallery = document.getElementById('gallery');

if (gallery) {
    gallery.addEventListener('slide.bs.carousel', function (e) {
        document.querySelectorAll('.gallery-tile').forEach(function (tile, index) {
            tile.classList.toggle('active', index === e.to);
        });

        const counter = document.getElementById('currentIndex');
        if (counter) counter.textContent = e.to + 1;
    });
}

document.getElementById('imageModal').addEventListener('show.bs.modal', function (e) {
    document.getElementById('modalImage').src = e.relatedTarged.src;
});
