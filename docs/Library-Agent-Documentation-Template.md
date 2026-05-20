# {{PackageName}} Agent Reference

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What {{PackageName}} is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

## Quick Facts

| Key | Value |
|---|---|
| Package | {{PackageName}} |
| Project file | [{{ProjectFilePath}}]({{ProjectFilePath}}) |
| Target frameworks | {{TargetFrameworksCsv}} |
| Primary role | {{PrimaryRole}} |
| Core themes | {{CoreThemesCsv}} |

## Use This Package When

- {{UseCase1}}
- {{UseCase2}}
- {{UseCase3}}
- {{UseCase4}}
- {{UseCase5}}

## Do Not Assume

- {{Constraint1}}
- {{Constraint2}}
- {{Constraint3}}

## High-Value Entry Points

### {{EntryPointGroup1Title}}

- {{ApiName1}} in [{{ApiFilePath1}}]({{ApiFilePath1}})
- {{ApiName2}} in [{{ApiFilePath2}}]({{ApiFilePath2}})
- {{ApiName3}} in [{{ApiFilePath3}}]({{ApiFilePath3}})
- {{ApiName4}} in [{{ApiFilePath4}}]({{ApiFilePath4}})

### {{EntryPointGroup2Title}}

- {{ApiName5}} in [{{ApiFilePath5}}]({{ApiFilePath5}})
- {{ApiName6}} in [{{ApiFilePath6}}]({{ApiFilePath6}})
- {{ApiName7}} in [{{ApiFilePath7}}]({{ApiFilePath7}})
- {{ApiName8}} in [{{ApiFilePath8}}]({{ApiFilePath8}})

### {{EntryPointGroup3Title}}

- {{ApiName9}} in [{{ApiFilePath9}}]({{ApiFilePath9}})
- {{ApiName10}} in [{{ApiFilePath10}}]({{ApiFilePath10}})
- {{ApiName11}} in [{{ApiFilePath11}}]({{ApiFilePath11}})
- {{ApiName12}} in [{{ApiFilePath12}}]({{ApiFilePath12}})

### {{EntryPointGroup4Title}}

- {{ApiName13}} in [{{ApiFilePath13}}]({{ApiFilePath13}})
- {{ApiName14}} in [{{ApiFilePath14}}]({{ApiFilePath14}})
- {{ApiName15}} in [{{ApiFilePath15}}]({{ApiFilePath15}})
- {{ApiName16}} in [{{ApiFilePath16}}]({{ApiFilePath16}})

### {{EntryPointGroup5Title}}

- {{ApiName17}} in [{{ApiFilePath17}}]({{ApiFilePath17}})
- {{ApiName18}} in [{{ApiFilePath18}}]({{ApiFilePath18}})
- {{ApiName19}} in [{{ApiFilePath19}}]({{ApiFilePath19}})

### {{EntryPointGroup6Title}}

- {{ApiName20}} in [{{ApiFilePath20}}]({{ApiFilePath20}})
- {{ApiName21}} in [{{ApiFilePath21}}]({{ApiFilePath21}})
- {{ApiName22}} in [{{ApiFilePath22}}]({{ApiFilePath22}})

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| {{Goal1}} | {{PreferredApi1}} | {{DecisionNote1}} |
| {{Goal2}} | {{PreferredApi2}} | {{DecisionNote2}} |
| {{Goal3}} | {{PreferredApi3}} | {{DecisionNote3}} |
| {{Goal4}} | {{PreferredApi4}} | {{DecisionNote4}} |
| {{Goal5}} | {{PreferredApi5}} | {{DecisionNote5}} |
| {{Goal6}} | {{PreferredApi6}} | {{DecisionNote6}} |
| {{Goal7}} | {{PreferredApi7}} | {{DecisionNote7}} |
| {{Goal8}} | {{PreferredApi8}} | {{DecisionNote8}} |

## Minimal Integration Recipe

### Install

```bash
dotnet add package {{PackageName}}
```

### Setup in Program.cs

```csharp
using {{Namespace1}};
using {{Namespace2}};

var builder = WebApplication.CreateBuilder(args);

{{BuilderSetupCode}}

{{ServiceRegistrationCode}}

{{OptionalInitializerOrConfigCode}}

var app = builder.Build();

{{PipelineSetupCode}}

app.Run();
```

## Behavior Notes That Affect Agent Decisions

- {{BehaviorNote1}}
- {{BehaviorNote2}}
- {{BehaviorNote3}}
- {{BehaviorNote4}}
- {{BehaviorNote5}}

## Source-Accurate Capability Map

### {{CapabilityArea1Title}}

- {{Capability1}}
- {{Capability2}}
- {{Capability3}}
- {{Capability4}}

Files:

- [{{CapabilityFile1}}]({{CapabilityFile1}})
- [{{CapabilityFile2}}]({{CapabilityFile2}})
- [{{CapabilityFile3}}]({{CapabilityFile3}})

### {{CapabilityArea2Title}}

- {{Capability5}}
- {{Capability6}}
- {{Capability7}}
- {{Capability8}}

Files:

- [{{CapabilityFile4}}]({{CapabilityFile4}})
- [{{CapabilityFile5}}]({{CapabilityFile5}})
- [{{CapabilityFile6}}]({{CapabilityFile6}})

### {{CapabilityArea3Title}}

- {{Capability9}}
- {{Capability10}}
- {{Capability11}}
- {{Capability12}}

Files:

- [{{CapabilityFile7}}]({{CapabilityFile7}})
- [{{CapabilityFile8}}]({{CapabilityFile8}})
- [{{CapabilityFile9}}]({{CapabilityFile9}})

### {{CapabilityArea4Title}}

- {{Capability13}}
- {{Capability14}}
- {{Capability15}}
- {{Capability16}}

Files:

- [{{CapabilityFile10}}]({{CapabilityFile10}})
- [{{CapabilityFile11}}]({{CapabilityFile11}})
- [{{CapabilityFile12}}]({{CapabilityFile12}})

### {{CapabilityArea5Title}}

- {{Capability17}}
- {{Capability18}}
- {{Capability19}}

Files:

- [{{CapabilityFile13}}]({{CapabilityFile13}})
- [{{CapabilityFile14}}]({{CapabilityFile14}})

### {{CapabilityArea6Title}}

- {{Capability20}}
- {{Capability21}}
- {{Capability22}}

Files:

- [{{CapabilityFile15}}]({{CapabilityFile15}})
- [{{CapabilityFile16}}]({{CapabilityFile16}})

## Dependencies

From [{{ProjectFilePath}}]({{ProjectFilePath}}):

- {{Dependency1}}
- {{Dependency2}}
- {{Dependency3}}
- {{Dependency4}}
- {{Dependency5}}

## Related Docs

- NuGet package readme: [{{NugetReadmePath}}]({{NugetReadmePath}})
- Repository guide: [README.md](README.md)
- Package documentation: [{{PackageDocumentationPath}}]({{PackageDocumentationPath}})

---

## Template Usage Notes

1. Keep section order unchanged so agents can parse docs consistently across packages.
2. Keep statements evidence-based and map each capability to concrete files.
3. Prefer API names exactly as implemented to improve retrieval quality.
4. Keep behavior notes focused on constraints that change agent decisions.
5. Use short, imperative wording in the decision matrix goal column.
6. Remove unused placeholders before publishing.

## Agent Authoring Checklist

- Scope is explicit: package role and non-goals are clearly separated.
- Entry points are complete: startup, runtime, domain, data, utility surfaces are covered.
- Decision matrix is actionable: each goal maps to one preferred API.
- Integration recipe compiles conceptually for the package type.
- Behavior notes include environment, lifecycle, and runtime caveats.
- Capability map links every claim to source files.
