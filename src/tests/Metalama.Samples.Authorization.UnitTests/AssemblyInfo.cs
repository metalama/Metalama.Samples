using Metalama.Samples.Authorization;

// The test types are securable, because they derive from RoleBasedSecurable. These tests check the run-time behavior
// of explicit [RequiresPermission] attributes, so default securing is disabled for the whole assembly. The aspect
// tests cover default securing.
[assembly: AuthorizationOptions( IsDefaultSecuringEnabled = false )]
