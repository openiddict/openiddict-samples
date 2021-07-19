import React from "react";
import { Route, Switch } from "react-router-dom";

import Admin from "./components/admin";
import Authorize from "./components/authorize";
import Footer from "./components/footer";
import Header from "./components/header";
import Home from "./components/home";
import Login from "./components/login";
import Register from "./components/register";

function App() {
  return (
    <div className="app">
      <Header />
      <Switch>
        <Route path="/admin" exact component={Admin} />
        <Route path="/register" exact component={Register} />
        <Route path="/login" exact component={Login} />
        <Route path="/authorize" exact component={Authorize} />
        <Route component={Home} />
      </Switch>
      <Footer />
    </div>
  );
}

export default App;
