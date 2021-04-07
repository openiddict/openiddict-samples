import { applyMiddleware, createStore, Store } from 'redux';
import { composeWithDevTools } from 'redux-devtools-extension';
import { createEpicMiddleware } from 'redux-observable-es6-compat';

import { rootEpic } from './rootEpic';
import { AppState, createAppReducer, getInitialAppState } from './rootReducer';

export const configureStore = (): Store<AppState, any> => {
  const epicMiddleware = createEpicMiddleware()

  const middlewares = [epicMiddleware]
  const middlewareEnhancer = applyMiddleware(...middlewares)

  const enhancers = [middlewareEnhancer]
  const composedEnhancers = composeWithDevTools(...enhancers)

  const store = createStore(
    createAppReducer(),
    getInitialAppState(),
    composedEnhancers
  )

  epicMiddleware.run(rootEpic)

  return store
}

export default configureStore
