// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombat.Lib;

public interface IRejectAbleEntity
{
    #region Public Properties

    public bool Rejected { get; }
    public string? RejectionDetails { get; }
    public string? RejectionReason { get; }

    #endregion
}