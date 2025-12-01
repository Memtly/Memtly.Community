import './sponsors.css';

import { displayLoader, hideLoader } from '../loader';
import { displayPopup } from '../popups';

export function initSponsors() {
    bindEventHandlers();
}

function bindEventHandlers() {
    $(document).off('click', '.btn-show-sponsors').on('click', '.btn-show-sponsors', function (e) {
        preventDefaults(e);

        displayLoader(localization.translate('Loading'));

        $.ajax({
            type: "GET",
            url: '/Sponsors',
            success: function (data) {
                hideLoader();
                displayPopup({
                    Title: localization.translate('Sponsors'),
                    CustomHtml: data,
                    Buttons: [{
                        Text: localization.translate('Sponsor'),
                        Class: 'btn-success',
                        Callback: function () {
                            displayPopup({
                                Title: localization.translate('Sponsors'),
                                CustomHtml: `<div class="text-center mb-5">
    	                                <section class="my-4">
    		                                <a href="https://github.com/sponsors/Cirx08" class="sponsor-card">
    			                                <img src="/images/github_avatar.png" class="sponsor-card-logo" alt="GitHub Sponsors Link" />
    			                                <p class="sponsor-card-name">GitHub Sponsors</p>
    		                                </a>
                                            <br/>
    		                                <a href="https://buymeacoffee.com/cirx08" class="sponsor-card">
    			                                <img src="/images/buymeacoffee_avatar.png" class="sponsor-card-logo" alt="BuyMeACoffee Sponsor Link" />
    			                                <p class="sponsor-card-name">BuyMeACoffee</p>
    		                                </a>
    	                                </section>
                                    </div>`,
                                Buttons: [{
                                    Text: localization.translate('Close')
                                }]
                            });
                        }
                    }, {
                        Text: localization.translate('Close')
                    }]
                });
            },
            error: function () {
                hideLoader();
            }
        });
    });
}