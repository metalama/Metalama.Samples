// Error UNIV01 on `Run`: `The type 'NotAnInterceptor' cannot give the universal interceptor because it does not implement IUniversalInterceptor.`
// Error UNIV02 on `Run`: `The type 'Interceptor' has no field or property named 'Missing'.`
// Error UNIV03 on `Run`: `The member 'Interceptor.InstanceMember' cannot give the universal interceptor because it is not static.`
// Error UNIV04 on `Run`: `The member 'Interceptor.WrongType' cannot give the universal interceptor because its type is 'Interceptor'. The type must be IUniversalInterceptor or AsyncLocal<IUniversalInterceptor>.`