import { AngularStarterPage } from './app.po';

describe('angular-starter App', function() {
  let page: AngularStarterPage;

  beforeEach(() => {
    page = new AngularStarterPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
