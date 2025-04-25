using System;
using Model;
using UnityEngine;
using Utils.Injection;

public enum CtaTag : byte
{
    None,
    HUDBuildMenu = 1,
    HUDResearch,
    BuildSectionResource = 10,
    BuildSectionStorage,
    BuildSectionSpecial,
    BuildMenuBuilding,
    RadialActionUpgrade = 20,
    RadialActionResearch,
    PlacedBuilding = 30,
    ResearchTypeBuilding = 40,
    ResearchTypeResource,
    ResearchTypePopulation,
    ResearchTypeFaith,
    Research = 50,
}

public class CtaTagMarker : InjectableBehaviour
{
    [Inject] private CtaRegister _register;

    [SerializeField] private CtaTag ctaTag = CtaTag.None;

    private void Start()
    {
        _register.Add(transform, ctaTag);
    }
}