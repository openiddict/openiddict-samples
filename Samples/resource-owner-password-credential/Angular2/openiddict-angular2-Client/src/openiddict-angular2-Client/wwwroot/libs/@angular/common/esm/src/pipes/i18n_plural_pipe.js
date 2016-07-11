import { Pipe } from '@angular/core';
import { StringWrapper, isPresent, isStringMap } from '../facade/lang';
import { InvalidPipeArgumentException } from './invalid_pipe_argument_exception';
const _INTERPOLATION_REGEXP = /#/g;
export class I18nPluralPipe {
    transform(value, pluralMap) {
        var key;
        var valueStr;
        if (!isStringMap(pluralMap)) {
            throw new InvalidPipeArgumentException(I18nPluralPipe, pluralMap);
        }
        key = value === 0 || value === 1 ? `=${value}` : 'other';
        valueStr = isPresent(value) ? value.toString() : '';
        return StringWrapper.replaceAll(pluralMap[key], _INTERPOLATION_REGEXP, valueStr);
    }
}
/** @nocollapse */
I18nPluralPipe.decorators = [
    { type: Pipe, args: [{ name: 'i18nPlural', pure: true },] },
];
//# sourceMappingURL=i18n_plural_pipe.js.map