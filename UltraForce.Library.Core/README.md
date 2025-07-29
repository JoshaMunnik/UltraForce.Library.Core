## Documentation

https://joshamunnik.github.io/UltraForce.Library.Core/index.html

## Version history
1.0.15
- `UFDataServiceFromContext.DetachEntity` now uses `WriteContext`
- updated some comments

1.0.14
- [BREAKING CHANGE] Updated UFDataServiceFromContext: replaced `Context` with `ReadContext`
and `WriteContext`
- reformatted code and renamed method parameters

1.0.13
- updated used packages

1.0.12
- created version history section in documentation
- added `UFCompareToAttribute`, `UFCompareOption` and `UFPropertyInfoExtensions`

1.0.11
- Removed class constrained

1.0.10
- Updated UFTestTools

1.0.9
- Updated used package.

1.0.8
- Include source comments.

1.0.7
- Updated IUFDataServiceCrud and IUFDataServiceFromContext to support immutable IUFDataServiceModel
  instances.

1.0.6
- Fixed bug in SendAsync

1.0.5
- Added aWaitForCompletion parameter to IUFEmailBuilder.SendAsync

1.0.4
- UFAzureEmailBuilderService caches and reuses azure email instance.

1.0.3
- Fixed bug in UFTestTools.AssertEqualList

1.0.2
- Added GetFromEmail to UFAzureEmailBuilderService

1.0.1
- IUFEmailBuilderService.Send now returns string instead of bool

1.0.0
- Initial version
