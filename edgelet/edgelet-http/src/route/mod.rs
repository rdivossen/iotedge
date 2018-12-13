// Copyright (c) Microsoft. All rights reserved.

/// This is inspired by [ubnu-intrepid's hyper router](https://github.com/ubnt-intrepid/hyper-router)
/// with some changes to improve usability of the captured parameters
/// when using regex based routes.
use std::clone::Clone;
use std::sync::Arc;

use failure::{Compat, Fail};
use futures::{future, Future};
use hyper::service::{NewService, Service};
use hyper::{Body, Method, Request, Response, StatusCode};
use url::form_urlencoded::parse as parse_query;

use error::{Error, ErrorKind};
use version::Version;
use IntoResponse;

pub mod macros;
mod regex;

pub type BoxFuture<T, E> = Box<Future<Item = T, Error = E>>;

pub trait Handler<P>: 'static + Send {
    fn handle(
        &self,
        req: Request<Body>,
        params: P,
    ) -> Box<Future<Item = Response<Body>, Error = Error> + Send>;
}

impl<F, P> Handler<P> for F
where
    F: 'static
        + Fn(Request<Body>, P) -> Box<Future<Item = Response<Body>, Error = Error> + Send>
        + Send,
{
    fn handle(
        &self,
        req: Request<Body>,
        params: P,
    ) -> Box<Future<Item = Response<Body>, Error = Error> + Send> {
        (*self)(req, params)
    }
}

pub type HandlerParamsPair<'a, P> = (&'a Handler<P>, P);

pub trait Recognizer {
    type Parameters: 'static;

    fn recognize(
        &self,
        method: &Method,
        version: &Version,
        path: &str,
    ) -> Result<HandlerParamsPair<Self::Parameters>, StatusCode>;
}

pub trait Builder: Sized {
    type Recognizer: Recognizer;

    fn route<S, H>(self, method: Method, version: Version, pattern: S, handler: H) -> Self
    where
        S: AsRef<str>,
        H: Handler<<Self::Recognizer as Recognizer>::Parameters> + Sync;

    fn finish(self) -> Self::Recognizer;

    fn get<S, H>(self, version: &str, pattern: S, handler: H) -> Self
    where
        S: AsRef<str>,
        H: Handler<<Self::Recognizer as Recognizer>::Parameters> + Sync,
    {
        self.route(Method::GET, version.parse::<Version>().unwrap(), pattern, handler)
    }

    fn post<S, H>(self, version: &str, pattern: S, handler: H) -> Self
    where
        S: AsRef<str>,
        H: Handler<<Self::Recognizer as Recognizer>::Parameters> + Sync,
    {
        self.route(Method::POST, version.parse::<Version>().unwrap(), pattern, handler)
    }

    fn put<S, H>(self, version: &str, pattern: S, handler: H) -> Self
    where
        S: AsRef<str>,
        H: Handler<<Self::Recognizer as Recognizer>::Parameters> + Sync,
    {
        self.route(Method::PUT, version.parse::<Version>().unwrap(), pattern, handler)
    }

    fn delete<S, H>(self, version: &str, pattern: S, handler: H) -> Self
    where
        S: AsRef<str>,
        H: Handler<<Self::Recognizer as Recognizer>::Parameters> + Sync,
    {
        self.route(Method::DELETE, version.parse::<Version>().unwrap(), pattern, handler)
    }
}

pub struct Router<R: Recognizer> {
    inner: Arc<R>,
}

impl<R: Recognizer> From<R> for Router<R> {
    fn from(recognizer: R) -> Self {
        Router {
            inner: Arc::new(recognizer),
        }
    }
}

impl<R> NewService for Router<R>
where
    R: Recognizer,
{
    type ReqBody = <Self::Service as Service>::ReqBody;
    type ResBody = <Self::Service as Service>::ResBody;
    type Error = <Self::Service as Service>::Error;
    type Service = RouterService<R>;
    type Future = future::FutureResult<Self::Service, Self::InitError>;
    type InitError = <Self::Service as Service>::Error;

    fn new_service(&self) -> Self::Future {
        future::ok(RouterService {
            inner: self.inner.clone(),
        })
    }
}

pub struct RouterService<R: Recognizer> {
    inner: Arc<R>,
}

impl<R> Clone for RouterService<R>
where
    R: Recognizer,
{
    fn clone(&self) -> Self {
        RouterService {
            inner: self.inner.clone(),
        }
    }
}

impl<R> Service for RouterService<R>
where
    R: Recognizer,
{
    type ReqBody = Body;
    type ResBody = Body;
    type Error = Compat<Error>;
    type Future = Box<Future<Item = Response<Self::ResBody>, Error = Self::Error> + Send>;

    fn call(&mut self, req: Request<Body>) -> Self::Future {

        let api_version =
        {
            let query = req.uri().query();
            query.and_then(|query| {
                let mut query = parse_query(query.as_bytes());
                let (_, api_version) = query.find(|&(ref key, _)| key == "api-version")?;
                
                let version = api_version.into_owned().parse::<Version>();

                match version 
                {
                    Ok(api_version) => Some(api_version),
                    Err(_) => None
                }
            })
        };

        match api_version {
                Some(ref api_version) => {
                    let method = req.method().clone();
                    let path = req.uri().path().to_owned();
                    match self.inner.recognize(&method, api_version, &path) {
                        Ok((handler, params)) => {
                            Box::new(handler.handle(req, params).map_err(|err| err.compat()))
                        }

                        Err(code) => Box::new(future::ok(
                            Response::builder()
                                .status(code)
                                .body(Body::empty())
                                .expect("hyper::Response with empty body should not fail to build"),
                        )),
                    }
                },
                None => Box::new(future::ok(Error::from(ErrorKind::InvalidApiVersion(String::new())).into_response())),
        }
    }
}

pub use route::regex::{Parameters, RegexRecognizer, RegexRoutesBuilder};

// #[cfg(test)]
// mod tests {
//     use failure::{Compat, Fail};
//     use futures::future::FutureResult;
//     use hyper::StatusCode;

//     use super::*;

//     #[derive(Clone)]
//     struct TestService {
//         status_code: StatusCode,
//         error: bool,
//     }

//     impl Service for TestService {
//         type ReqBody = Body;
//         type ResBody = Body;
//         type Error = Compat<Error>;
//         type Future = FutureResult<Response<Self::ResBody>, Self::Error>;

//         fn call(&mut self, _req: Request<Self::ReqBody>) -> Self::Future {
//             if self.error {
//                 future::err(Error::from(ErrorKind::ServiceError).compat())
//             } else {
//                 future::ok(
//                     Response::builder()
//                         .status(self.status_code)
//                         .body(Body::default())
//                         .unwrap(),
//                 )
//             }
//         }
//     }

//     #[test]
//     fn api_version_check_succeeds() {
//         let url = &format!("http://localhost?api-version={}", API_VERSION);
//         let req = Request::get(url).body(Body::default()).unwrap();
//         let mut api_service = ApiVersionService::new(TestService {
//             status_code: StatusCode::OK,
//             error: false,
//         });
//         let response = Service::call(&mut api_service, req).wait().unwrap();
//         assert_eq!(StatusCode::OK, response.status());
//     }

//     #[test]
//     fn api_version_check_passes_status_code_through() {
//         let url = &format!("http://localhost?api-version={}", API_VERSION);
//         let req = Request::get(url).body(Body::default()).unwrap();
//         let mut api_service = ApiVersionService::new(TestService {
//             status_code: StatusCode::IM_A_TEAPOT,
//             error: false,
//         });
//         let response = Service::call(&mut api_service, req).wait().unwrap();
//         assert_eq!(StatusCode::IM_A_TEAPOT, response.status());
//     }

//     #[test]
//     fn api_version_check_returns_error_as_response() {
//         let url = &format!("http://localhost?api-version={}", API_VERSION);
//         let req = Request::get(url).body(Body::default()).unwrap();
//         let mut api_service = ApiVersionService::new(TestService {
//             status_code: StatusCode::IM_A_TEAPOT,
//             error: true,
//         });
//         let response = Service::call(&mut api_service, req).wait().unwrap();
//         assert_eq!(StatusCode::INTERNAL_SERVER_ERROR, response.status());
//     }

//     #[test]
//     fn api_version_does_not_exist() {
//         let url = "http://localhost";
//         let req = Request::get(url).body(Body::default()).unwrap();
//         let mut api_service = ApiVersionService::new(TestService {
//             status_code: StatusCode::OK,
//             error: false,
//         });
//         let response = Service::call(&mut api_service, req).wait().unwrap();
//         assert_eq!(StatusCode::BAD_REQUEST, response.status());
//     }

//     #[test]
//     fn api_version_is_unsupported() {
//         let url = "http://localhost?api-version=not-a-valid-version";
//         let req = Request::get(url).body(Body::default()).unwrap();
//         let mut api_service = ApiVersionService::new(TestService {
//             status_code: StatusCode::OK,
//             error: false,
//         });
//         let response = Service::call(&mut api_service, req).wait().unwrap();
//         assert_eq!(StatusCode::BAD_REQUEST, response.status());
//     }
// }