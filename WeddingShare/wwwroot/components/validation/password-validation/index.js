import { generateChecklistItem } from '../html-elements';

function bindEventHandlers() {
    if ($('div.password-validator-container').length > 0) {
        $('div.password-validator-container').each(function () {
            $(this).html(generatePasswordValidation($(this).data('input')));
        });
    }

    initPasswordValidation();
}

export function initPasswordValidation() {
    if ($('.password-validator').length > 0) {
        $('.password-validator').each(function () {
            let validator = $(this);
            let input = $(validator.data('input'));
            if (input !== undefined && input.length > 0) {
                let confirmField = input.parent().parent().parent().find('input.confirm-password');
                if (confirmField !== undefined && confirmField.length === 1) {
                    validator.find('.lbl-confirm').removeClass('visually-hidden');
                    confirmField.off('keyup').on('keyup', function () {
                        var value = $(input).val();
                        setPasswordValidationField(validator.find('.lbl-confirm'), confirmField.val() === value && value.length);
                        setPasswordValidationField(validator, validator.find('li[class^=\'lbl-\']:not([class*=\'hidden\'])').length === 0);
                    });
                }

                $(input).off('keyup').on('keyup', function () {
                    var value = $(this).val();
                    setPasswordValidationField(validator.find('.lbl-lower'), /[a-z]+?/.test(value));
                    setPasswordValidationField(validator.find('.lbl-upper'), /[A-Z]+?/.test(value));
                    setPasswordValidationField(validator.find('.lbl-number'), /[0-9]+?/.test(value));
                    setPasswordValidationField(validator.find('.lbl-special'), /[^A-Za-z0-9]+?/.test(value));
                    setPasswordValidationField(validator.find('.lbl-length'), value.length >= 8);

                    if (confirmField !== undefined && confirmField.length === 1) {
                        setPasswordValidationField(validator.find('.lbl-confirm'), confirmField.val() === value && value.length);
                    }

                    setPasswordValidationField(validator, validator.find('li[class^=\'lbl-\']:not([class*=\'hidden\'])').length === 0);
                })
            }
        });
    }
}

export function generatePasswordValidation(field) {
    return `
        <div class="checklist password-validator" data-input="${field}">
            ${generateChecklistItem('lbl-lower', 'default', localization.translate('Password_Validation_Lower'))}
            ${generateChecklistItem('lbl-upper', 'default', localization.translate('Password_Validation_Upper'))}
            ${generateChecklistItem('lbl-number', 'default', localization.translate('Password_Validation_Numbers'))}
            ${generateChecklistItem('lbl-special', 'default', localization.translate('Password_Validation_Special'))}
            ${generateChecklistItem('lbl-length', 'default', localization.translate('Password_Validation_Length'))}
            ${generateChecklistItem('lbl-confirm', 'default', localization.translate('Password_Validation_Confirm'), true)}
        </div>
    `;
}

function setPasswordValidationField(field, valid) {
    if (valid) {
        field.removeClass('checklist-success checklist-error checklist-default');
        field.addClass('checklist-success');
        field.find('i.fa').removeClass('fa-square-check fa-square-xmark fa-square fa-regular');
        field.find('i.fa').addClass('fa-square-check');
    } else {
        field.removeClass('checklist-success checklist-error checklist-default');
        field.addClass('checklist-error');
        field.find('i.fa').removeClass('fa-square-check fa-square-xmark fa-square fa-regular');
        field.find('i.fa').addClass('fa-square-xmark');
    }
}

bindEventHandlers();