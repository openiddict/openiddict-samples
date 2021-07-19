import { Action } from "redux";
import {
  ActionsObservable,
  combineEpics,
  Epic,
  StateObservable,
} from "redux-observable";
import { catchError } from "rxjs/operators";

import { adminEpics } from "./admin/adminEpics";
import { testEpics } from "./test/testEpics";
import { userEpics } from "./user/userEpics";

var topLevelEpics: Array<Epic> = [...userEpics, ...adminEpics, ...testEpics];

export const rootEpic = (
  action$: ActionsObservable<Action>,
  store$: StateObservable<any>,
  dependencies: any
) =>
  combineEpics(...topLevelEpics)(action$, store$, dependencies).pipe(
    catchError((error, source) => {
      console.error(error);
      return source;
    })
  );
