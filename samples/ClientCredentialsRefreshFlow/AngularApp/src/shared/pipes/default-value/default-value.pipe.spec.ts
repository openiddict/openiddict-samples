import { DefaultValuePipe } from './default-value.pipe';

describe('BannerComponent', () => {

    let pipe = new DefaultValuePipe();
    it('should not return the default value', () => {
        expect(pipe.transform('toxicable', 'no name specified')).toBe('toxicable');
    });
    it('should return the default value', () => {
        expect(pipe.transform('', 'no name specified')).toBe('no name specified');
    });

    it('should return the default value', () => {
        expect(pipe.transform(undefined, 'no name specified')).toBe('no name specified');
    });


});
