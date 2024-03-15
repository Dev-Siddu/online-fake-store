window.addEventListener('DOMContentLoaded', function () {
    let cancelBtn = this.document.querySelector('.sidebar > #cancel');
    let hamBtn = this.document.querySelector('#hamberger');
    let mainContent = this.document.querySelector('#body-content');

    cancelBtn.addEventListener('click', function () {
        let sidebarele = document.querySelector('#sidebar');
        sidebarele.style.left = "-300px";
        mainContent.style.left = "0px";
        //document.querySelector('#hamberger').classList.toggle('display-none');
    });

    hamBtn.addEventListener('click', function () {
        let sidebarele = document.querySelector('#sidebar');
        sidebarele.style.left = "0px";
        //mainContent.style.left = "200px";
        //this.classList.toggle('display-none');

    });

});
