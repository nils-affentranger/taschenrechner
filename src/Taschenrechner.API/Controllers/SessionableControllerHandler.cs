﻿using System.Web;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace Taschenrechner.api.Controller {

    public class SessionControllerHandler : HttpControllerHandler, IRequiresSessionState {

        public SessionControllerHandler(RouteData routeData)
        : base(routeData) { }
    }

    public class SessionStateRouteHandler : IRouteHandler {

        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext) {
            return new SessionControllerHandler(requestContext.RouteData);
        }
    }
}