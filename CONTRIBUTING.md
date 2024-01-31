# Contributing Guidelines

Thank you for your interest in contributing to Babylon Network Gateway! 

## Clarification on GitHub Issue Usage and Feature Requests

We want to clarify that Github Issues are primarily meant for the purpose of reporting problems or concerns, rather than functioning as an open bug tracker. This means that reported issues on Github may be closed and reported in our internal tracking system or added to our roadmap. 

If you are thinking of requesting a feature, make sure it’s not already part of our upcoming features outlined in the [Roadmap](https://docs.radixdlt.com/docs/roadmap). If you have a feature suggestion, we kindly ask that you share it through [Discord](http://discord.gg/radixdlt) or [Telegram](https://t.me/RadixDevelopers).

Our primary focus is on the priorities outlined in our Roadmap. We appreciate your understanding that addressing reported issues may not always align with our immediate roadmap goals.


# Table of Contents
1. [Code of Conduct](#code-of-conduct)
2. [Reporting Issues](#reporting-issues)
3. [Contributing Code](#contributing-code)
   - [Setting Up Your Development Environment](#setting-up-your-development-environment)
   - [Making Changes](#making-changes)
   - [Testing](#testing)
   - [Submitting a Pull Request](#submitting-a-pull-request)
5. [Review Process](#review-process)
6. [Code Style](#code-style)
7. [License](#license)

# Code of Conduct
This project adheres to the Contributor Covenant [code of conduct](CODE_OF_CONDUCT.md).
By participating, you are expected to uphold this code.
Please report unacceptable behavior to [hello@radixdlt.com](mailto:hello@radixdlt.com).

# Reporting Issues
Ensure the bug was not already reported by searching on GitHub under [Issues](https://github.com/radixdlt/babylon-gateway/issues).

If you encounter a bug or have a problem with the project, please open an issue on our [issue tracker](https://github.com/radixdlt/babylon-gateway/issues). Make sure to provide as much detail as possible, including:

- A clear and descriptive title.
- Steps to reproduce the issue.
- Expected behavior and actual behavior.
- Your operating system, browser, or other relevant information.
- If possible, include screenshots or code snippets that illustrate the issue.


# Contributing Code

Prior to commencing any work on a PR, we strongly advise initiating a discussion with the team via Discord, Telegram, or Github Issues (for bugs).

Submitting a Pull Request does not guarantee the acceptance of your proposed changes.

## Setting Up Your Development Environment

Please check [documentation](https://github.com/radixdlt/babylon-gateway/blob/main/docs/development.md) on how to setup local environment.

## Making Changes

1. Write clear, concise, and well-documented code.
2. Commit your changes with a descriptive commit message. Please follow the PROJECT’s commit message format.

## Testing

1. Ensure that your changes do not break existing tests (`/tests` directory)
2. Write new tests for your code if applicable.
3. Run the test suite to make sure everything is passing (you can do that by using `dotnet test` command or any IDE of your choice)

## Submitting a Pull Request

1. Push your changes to your forked repository:
2. Open a pull request against the `develop` branch of the original repository.
3. Provide a clear and informative title and description for your pull request.
4. Be prepared to address any feedback or questions during the review process.

# Review Process
Pull requests will be reviewed by project maintainers. Reviewers may provide feedback, request changes, or approve the pull request. We appreciate your patience during this process, and we aim to be responsive and constructive in our feedback.


# License
By contributing to Babylon Network Gateway, you agree that your contributions will be licensed under the Babylon Network Gateway license.

The executable components of the Babylon Gateway Code are licensed under the [Radix Software EULA](http://www.radixdlt.com/terms/genericEULA).

The Babylon Gateway Code is released under the [Radix License 1.0 (modified Apache 2.0)](LICENSE):

```
Copyright 2023 Radix Publishing Ltd incorporated in Jersey, Channel Islands.

Licensed under the Radix License, Version 1.0 (the "License"); you may not use
this file except in compliance with the License.

You may obtain a copy of the License at:
https://www.radixfoundation.org/licenses/license-v1

The Licensor hereby grants permission for the Canonical version of the Work to
be published, distributed and used under or by reference to the Licensor’s
trademark Radix® and use of any unregistered trade names, logos or get-up.

The Licensor provides the Work (and each Contributor provides its Contributions)
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
express or implied, including, without limitation, any warranties or conditions
of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR
PURPOSE.

Whilst the Work is capable of being deployed, used and adopted (instantiated) to
create a distributed ledger it is your responsibility to test and validate the
code, together with all logic and performance of that code under all foreseeable
scenarios.

The Licensor does not make or purport to make and hereby excludes liability for
all and any representation, warranty or undertaking in any form whatsoever,
whether express or implied, to any entity or person, including any
representation, warranty or undertaking, as to the functionality security use,
value or other characteristics of any distributed ledger nor in respect the
functioning or value of any tokens which may be created stored or transferred
using the Work.

The Licensor does not warrant that the Work or any use of the Work complies with
any law or regulation in any territory where it may be implemented or used or
that it will be appropriate for any specific purpose.

Neither the licensor nor any current or former employees, officers, directors,
partners, trustees, representatives, agents, advisors, contractors, or
volunteers of the Licensor shall be liable for any direct or indirect, special,
incidental, consequential or other losses of any kind, in tort, contract or
otherwise (including but not limited to loss of revenue, income or profits, or
loss of use or data, or loss of reputation, or loss of any economic or other
opportunity of whatsoever nature or howsoever arising), arising out of or in
connection with (without limitation of any use, misuse, of any ledger system or
use made or its functionality or any performance or operation of any code or
protocol caused by bugs or programming or logic errors or otherwise);

A. any offer, purchase, holding, use, sale, exchange or transmission of any
cryptographic keys, tokens or assets created, exchanged, stored or arising from
any interaction with the Work;

B. any failure in a transmission or loss of any token or assets keys or other
digital artifacts due to errors in transmission;

C. bugs, hacks, logic errors or faults in the Work or any communication;

D. system software or apparatus including but not limited to losses caused by
errors in holding or transmitting tokens by any third-party;

E. breaches or failure of security including hacker attacks, loss or disclosure
of password, loss of private key, unauthorised use or misuse of such passwords
or keys;

F. any losses including loss of anticipated savings or other benefits resulting
from use of the Work or any changes to the Work (however implemented).

You are solely responsible for; testing, validating and evaluation of all
operation logic, functionality, security and appropriateness of using the Work
for any commercial or non-commercial purpose and for any reproduction or
redistribution by You of the Work. You assume all risks associated with Your use
of the Work and the exercise of permissions under this Licence.
```

