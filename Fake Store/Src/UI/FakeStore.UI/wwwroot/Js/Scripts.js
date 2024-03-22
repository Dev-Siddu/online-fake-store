window.addEventListener('DOMContentLoaded', function () {
    let cancelBtn = this.document.querySelector('.sidebar > #cancel');
    let hamBtn = this.document.querySelector('#hamberger');
    let mainContent = this.document.querySelector('#body-content');

    cancelBtn.addEventListener('click', function () {
        let sidebarele = document.querySelector('#sidebar');
        sidebarele.style.left = "-300px";
        mainContent.style.left = "0px";
    });

    hamBtn.addEventListener('click', function () {
        let sidebarele = document.querySelector('#sidebar');
        sidebarele.style.left = "0px";
    });

    document.addEventListener('keypress', function (e) {
        if (e.key == 'm' || e.key == 'M') {
            console.log(e.key);
            let sidebarele = document.querySelector('#sidebar');
            sidebarele.style.left = "0px";
        }
        else if (e.key == 'x' || e.Key == 'X') {
            console.log(e.key);
            let sidebarele = document.querySelector('#sidebar');
            sidebarele.style.left = "-300px";
            mainContent.style.left = "0px";
        }
    });
});
