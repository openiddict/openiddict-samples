import { provide, Component, DynamicComponentLoader,Query, ViewContainerRef, Injector, Type } from '@angular/core';
declare var System: any;
export class ComponentProvider {
    path: string;
    provide: { (module: any): any };
}

const PROXY_CLASSNAME = 'component-wrapper';
const PROXY_SELECTOR = `.${PROXY_CLASSNAME}`;

export function componentProxyFactory(provider: ComponentProvider): Type {
    @Component({
        selector: 'component-proxy',
        template: `<span #content></span>`,
        providers: [provide(ComponentProvider, { useValue: provider })]
    })


    class VirtualComponent {
        constructor(
            el: ViewContainerRef,
            loader: DynamicComponentLoader,
            inj: Injector,
            provider: ComponentProvider) {
            System.import(provider.path)
                .then(m => {
                    loader.loadNextToLocation(provider.provide(m), el);
                });
        }
    }
    return VirtualComponent;
}