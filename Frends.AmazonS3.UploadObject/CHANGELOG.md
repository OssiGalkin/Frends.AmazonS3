# Changelog

## [1.1.1] - 2023-06-13
### Changed
- Connection.ThrowExceptionOnErrorResponse documentation update.

## [1.1.0] - 2023-02-07
### Changed
- Task returns an object instead of list.
### Added
- Result.Success to indicate that operation was completed without errors.
- Result.DebugLog to contain log of each operation.
- Connection.ThrowExceptionOnErrorResponse to choose whether an error throws an exception or is logged to Result.DebugLog.

## [1.0.0] - 2022-05-05
### Added
- Initial implementation