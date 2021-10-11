# TODO

* Find and fix all FIXME's
* Determine if job Allocators should be perm or TempJob
* Determine if jobs should take in smaller pieces so as to mark READONLY (perf check)
  * Ensure timing instrumenting is in place BEFORE to do before/after comparisson
  * This will require minor refactoring of all jobs to take in just the native elements necessary