import 'bootstrap/dist/css/bootstrap.css';
import './index.css';

import { ConnectedRouter as Router } from 'connected-react-router/immutable';
import { createBrowserHistory } from 'history';
import { UserManagerSettings } from 'oidc-client';
import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';

import App from './app/App';
import { authUrl } from './app/environments/config';
import configureStore from './app/redux/store';
import reportWebVitals from './reportWebVitals';

const oidcSettings: UserManagerSettings = {
    authority: authUrl,
    client_id: 'ralltiir-client',
    redirect_uri: 'https://localhost:5001/oidc',
    scope: 'openid offline_access profile roles',
    response_type: 'code',
    automaticSilentRenew: true
  };

const history = createBrowserHistory();
const store = configureStore(history, oidcSettings)

ReactDOM.render(
  <React.StrictMode>
    <Provider store={store}>
      <Router history={history}>
          <App />
      </Router>
    </Provider>
  </React.StrictMode>,
  document.getElementById('root')
)

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals()
