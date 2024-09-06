// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

namespace zxeltor.StoCombat.Lib.Model.CombatLog;

/// <summary>
///     A wrapper class for combat entity identifiers, used by Map Detection logic.
/// </summary>
public class CombatEntityLabel : IEqualityComparer<CombatEntityLabel>, IEquatable<CombatEntityLabel>
{
    #region Constructors

    /// <summary>
    ///     A constructor using parameters for <see cref="Id" /> and <see cref="IsPlayer" />
    /// </summary>
    /// <param name="id">The id of the combat entity. Player or Non-Player</param>
    /// <param name="label">The display label of the combat entity. Player or Non-Player</param>
    /// <param name="isPet">If true the id is for a pet. False otherwise.</param>
    /// <param name="ownerId">If this combat entity is a pet, this should be set with the id of the Player or Non-Player entity which owns the pet.</param>
    /// <param name="ownerLabel">If this combat entity is a pet, this should be set with the display label of the Player or Non-Player entity which owns the pet.</param>
    public CombatEntityLabel(string id, string label,
        bool isPet = false, string? ownerId = null, string? ownerLabel = null)
    {
        this.Id = id;
        //this.IdParsed = idParsed;
        this.Label = label;
        this.IsPet = isPet;
        this.OwnerId = ownerId;
        this.OwnerLabel = ownerLabel;
    }

    #endregion

    #region Public Properties

    /// <summary>
    ///     The id of the combat entity. Player or Non-Player
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The display label of the combat entity. Player or Non-Player
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     If this combat entity is a pet, this will be set with the id of the Player or Non-Player entity which owns the pet.
    /// </summary>
    public string? OwnerId { get; set; }

    /// <summary>
    ///     If this combat entity is a pet, this will be set with the display label of the Player or Non-Player entity which
    ///     owns the pet.
    /// </summary>
    public string? OwnerLabel { get; set; }

    /// <summary>
    ///     If true the id is for a pet. False otherwise.
    /// </summary>
    public bool IsPet { get; set; }

    #endregion

    #region Public Members

    public bool Equals(CombatEntityLabel x, CombatEntityLabel y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id && x.Label == y.Label;
    }

    public int GetHashCode(CombatEntityLabel obj)
    {
        return HashCode.Combine(obj.Id, obj.Label);
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Label={this.Label}, Id={this.Id}, IsPet={this.IsPet}";
    }

    #endregion

    #endregion

    #region Equality members

    /// <inheritdoc />
    public bool Equals(CombatEntityLabel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.Id == other.Id && this.Label == other.Label;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return this.Equals((CombatEntityLabel)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id, this.Label);
    }

    #endregion
}