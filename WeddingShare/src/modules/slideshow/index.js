class Slideshow {
    constructor(selector, slideInterval = 10000, fadeInterval = 500) {
        this.elem = $(selector);
        this.slidetimer = null;
        this.transitionTimer = null;
        this.currentSlide = 0;
        this.slideInterval = slideInterval;
        this.fadeInterval = fadeInterval;
    }

    init() {
        if (this.elem !== undefined) {
            this.currentSlide = 0;

            const slideCount = this.elem.find('.slideshow-slide').length;
            if (slideCount > 0) {
                const windowHeight = $(window).outerHeight();
                const headerHeight = $('.navbar').outerHeight();
                const footerHeight = $('footer').outerHeight();
                const creditsHeight = $('.credits').length > 0 ? 20 : 0;
                const reviewCounterHeight = $('.review-counter').length > 0 ? $('.review-counter').outerHeight() + 70 : 50;
                const slideHeight = windowHeight - (headerHeight + footerHeight + reviewCounterHeight + creditsHeight);

                const qrCodeVal = this.elem.find('.slideshow-slide .share-slide').attr('data-value');
                this.elem.find('.slideshow-slide .share-slide').qrcode({ width: slideHeight, height: slideHeight, text: qrCodeVal });

                this.elem.height(slideHeight);
                this.elem.find('.slideshow-slide').each(function (index) {
                    $(this).attr('data-slide-index', index);
                });
                this.elem.find('.slideshow-slide[data-slide-index="0"]').show();

                clearInterval(this.slidetimer);
                this.slidetimer = setInterval(() => {
                    this.currentSlide++;

                    if (this.currentSlide >= slideCount) {
                        $.ajax({
                            type: 'GET',
                            url: `${window.location.pathname}${window.location.search}&partial=true`,
                            success: (data) => {
                                clearInterval(this.slidetimer);
                                clearTimeout(this.transitionTimer);
                                $('#main-gallery').html(data);
                                this.init(this.fadeInterval);
                            }
                        });
                    }

                    this.elem.find('.slideshow-slide').fadeOut(this.fadeInterval);
                    clearTimeout(this.transitionTimer);
                    this.transitionTimer = setTimeout(() => {
                        this.elem.find(`.slideshow-slide[data-slide-index="${this.currentSlide}"]`).fadeIn(this.fadeInterval);
                    }, this.fadeInterval);
                }, this.slideInterval);
            }
        }
    }
}

export default Slideshow;