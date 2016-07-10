import { Component, ChangeDetectorRef} from '@angular/core';
import {geoService} from './geoservice';
import {GeoSharedService, geoSharedModel, geoResult} from './geo.shared';
declare var google: any;

@Component({
    selector: 'my-location',
    template: ` <li>
                    <form>
                            <div class="input-field">
                                <input id="google_places_ac" [(attr.state)]="geoLocation.state" [(attr.country)]="geoLocation.coutnry" name="google_places_ac" type="text" value="{{geoLocation.city}}" class="form-control" />
                                <label class="active" for="google_places_ac"><i class="small material-icons" style="font-size: 20px; color:#000000">my_location</i></label>
                            </div>
                    </form>
                </li>`,
    directives: [],
    providers: [ ]
})

export class geoComponent {

    public geoMapper: geoResult;
    public geoLocation = new geoSharedModel;
    public input: any;
    public errorMessage: string;

    constructor(private _geoService: geoService,
        private geosharedService: GeoSharedService,
        private cdr: ChangeDetectorRef) { }

    //oninit
    ngOnInit() {
        var instance = this,//!important as javascript "this" issue may cause some issues
            autocomplete;

        if (navigator.geolocation) { // iF BROWSER location is enabled by user
            navigator.geolocation.getCurrentPosition(res => {
                instance._geoService.getPlaces(res.coords.latitude, res.coords.longitude).subscribe(
                    data => {
                        instance.geoMapper = <geoResult>data;
                        console.log(instance.geoMapper);
                        var result = instance.geoMapper.status;
                        var result2 = instance.geoMapper.results;
                        if (result != "undefined" || "" || null) {
                            instance.setValue(result2[0].address_components[6].long_name, result2[0].address_components[5].long_name, result2[0].address_components[4].long_name);
                        }

                    });
            });
        }
        //below code works if user enters any value in textbox nonethless if navigation is enabled or not
        instance.input = document.getElementById('google_places_ac');
        autocomplete = new google.maps.places.Autocomplete(instance.input, { types: ['(cities)'] });
        google.maps.event.addListener(autocomplete, 'place_changed', function () {
            var place = autocomplete.getPlace(); console.log(place);
            if (place.address_components[3] != undefined) {
                instance.setValue(place.address_components[3].long_name, place.address_components[2].long_name, place.address_components[1].long_name);
            } else {
                instance.setValue(place.address_components[2].long_name, place.address_components[1].long_name, place.address_components[0].long_name);
            }
        });
    }

    //function to set values in shared service so we can access it anytime
    setValue(a, b, c) {
        this.geoLocation.country = a;
        this.geoLocation.state = b;
        this.geoLocation.city = c;
        this.geosharedService.broadcastTextChange(this.geoLocation);
        this.cdr.detectChanges();
    }
}