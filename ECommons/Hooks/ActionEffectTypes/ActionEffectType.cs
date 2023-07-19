﻿//This file is authored by lmcintyre and distributed under GNU GPL v3 license. https://github.com/lmcintyre/

using System;

namespace ECommons.Hooks.ActionEffectTypes;

public enum ActionEffectType : byte
{
    Nothing = 0,
    Miss = 1,
    FullResist = 2,
    Damage = 3,
    Heal = 4,
    BlockedDamage = 5,
    ParriedDamage = 6,
    Invulnerable = 7,
    NoEffectText = 8,
    Unknown_0 = 9,
    MpLoss = 10,
    MpGain = 11,
    TpLoss = 12,
    TpGain = 13,
    ApplyStatusEffectTarget = 14,
    ApplyStatusEffectSource = 15,
    RecoveredFromStatusEffect = 16,
    LoseStatusEffectTarget = 17,
    LoseStatusEffectSource = 18,
    StatusNoEffect = 20,
    ThreatPosition = 24,
    EnmityAmountUp = 25,
    EnmityAmountDown = 26,

    [Obsolete("Please use StartActionCombo instead.")]
    Unknown0 = StartActionCombo,
    StartActionCombo = 27,
    Unknown1 = 28,
    Retaliation = 29,
    Knockback = 32,
    Knockback2 = 33,
    Mount = 40,
    FullResistStatus = 52,
    FullResistStatus2 = 55,
    VFX = 59,
    Gauge = 60,
    JobGauge = 61,
    SetModelState = 72,
    SetHP = 73,
    PartialInvulnerable = 74,
    Interrupt = 75,
};
