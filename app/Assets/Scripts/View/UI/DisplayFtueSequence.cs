using System;
using System.Collections;
using Model;
using Notifications;
using Settlement.Types;
using UnityEngine;
using Utils;
using Utils.Injection;
using View.UI.Building;
using View.UI.Research;

namespace View.UI
{
    public class DisplayFtueSequence : InjectableBehaviour
    {
        [Inject] StartFtueSequence _startFtueSequence;
        [Inject] StopFtueSequence _stopFtueSequence;
        [Inject] CtaRegister _ctaRegister;
        [Inject] ConfigModel _config;

        [Inject] ShowFtuePrompt _showFtue;
        [Inject] HideFtuePrompt _hideFtue;
        [Inject] FocusOn _focusOn;

        [Inject] NavigationContextModel _nav;
        [Inject] private GridInteractionStateModel _interaction;

        private void Start()
        {
            _startFtueSequence.Add(OnStartFtue);
            _stopFtueSequence.Add(OnStopFtue);
        }

        private void OnStartFtue(QuestData questData)
        {
            StartCoroutine(DisplayTutorial(questData));
        }

        private void OnStopFtue()
        {
            _hideFtue.Dispatch();
            StopAllCoroutines();
        }

        private IEnumerator DisplayTutorial(QuestData questData)
        {
            switch (questData.type)
            {
                case QuestType.Build:
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.right,
                            promptText = "Open <color=green>Build Menu</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.HUDBuildMenu)
                        }
                    });

                    yield return new WaitUntil(() => _nav.IsBuildMenuOpen);
                    yield return null;
                    _hideFtue.Dispatch();

                    var buildingType = (BuildingType)questData.targetType;
                    var buildingFilter = _config.BuildingTypeMapping[buildingType];

                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{buildingFilter}</color> Buildings",
                            cutoutScreenSpace = GetScreenRect(buildingFilter switch
                            {
                                BuildingFilter.ResourceCollection => CtaTag.BuildSectionResource,
                                BuildingFilter.Storage => CtaTag.BuildSectionStorage,
                                BuildingFilter.Special => CtaTag.BuildSectionSpecial,
                                _ => throw new ArgumentOutOfRangeException()
                            })
                        }
                    });

                    yield return new WaitUntil(() => _nav.BuildingFilter == buildingFilter);

                    yield return null;
                    _hideFtue.Dispatch();
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{buildingType}</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.BuildMenuBuilding, questData.targetType)
                        }
                    });

                    yield return new WaitUntil(() => _interaction.SelectedBuildingType == buildingType);
                    _hideFtue.Dispatch();
                    break;
                case QuestType.Upgrade:

                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            cutoutScreenSpace = Rect.zero,
                        }
                    });

                    buildingType = (BuildingType)questData.targetType;
                    var building = _ctaRegister.Get(CtaTag.PlacedBuilding, (int?)buildingType);
                    var buildingInfo = building.GetComponent<BuildingInfo>();
                    _focusOn.Dispatch(buildingInfo.worldAnchor.position);

                    yield return new WaitForSeconds(.3f);

                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{buildingType}</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.PlacedBuilding, questData.targetType)
                        }
                    });
                    
                    yield return new WaitUntil(() => buildingInfo.controls.activeSelf);
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            cutoutScreenSpace = Rect.zero,
                        }
                    });
                    yield return new WaitForSeconds(.2f);
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Click <color=green>Upgrade</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.RadialActionUpgrade)
                        }
                    });
                    
                    break;
                case QuestType.Store:
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = false,
                            promptLocation = Vector2Int.down,
                            promptText = "If a workers are assigned to a <i>resource collection</i> building,\nthey would collect given resources every <color=green>turn</color>, up to the storage capacity",
                            cutoutScreenSpace = GetScreenRect(CtaTag.HUDTreasury)
                        }
                    });
                    break;
                case QuestType.Research:
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            cutoutScreenSpace = Rect.zero,
                        }
                    });

                    building = _ctaRegister.Get(CtaTag.PlacedBuilding, (int?)BuildingType.Research);
                    buildingInfo = building.GetComponent<BuildingInfo>();
                    _focusOn.Dispatch(buildingInfo.worldAnchor.position);

                    yield return new WaitForSeconds(.3f);

                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{BuildingType.Research}</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.PlacedBuilding, (int)BuildingType.Research)
                        }
                    });
                    
                    yield return new WaitUntil(() => buildingInfo.controls.activeSelf);
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            cutoutScreenSpace = Rect.zero,
                        }
                    });
                    yield return new WaitForSeconds(.2f);
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Click <color=green>Research</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.RadialActionResearch)
                        }
                    });
                    
                    yield return new WaitUntil(() => _nav.IsResearchOpen);
                    
                    yield return null;
                    yield return null;

                    var questResearchType = (SettlementModel.ResearchType)questData.targetType;
                    var researchFilter = _config.ResearchTypeMapping[questResearchType];
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{researchFilter}</color>",
                            cutoutScreenSpace = GetScreenRect(researchFilter switch
                            {
                                ResearchFilter.Building => CtaTag.ResearchTypeBuilding,
                                ResearchFilter.Resource => CtaTag.ResearchTypeResource,
                                ResearchFilter.Population => CtaTag.ResearchTypePopulation,
                                ResearchFilter.Faith => CtaTag.ResearchTypeFaith,
                                _ => throw new ArgumentOutOfRangeException()
                            })
                        }
                    });
                    
                    
                    
                    yield return new WaitUntil(() => _nav.ResearchFilter == researchFilter);
                    yield return null;
                    yield return null;
                    
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>{questResearchType.ToString().SplitCamelCase()}</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.Research, questData.targetType)
                        }
                    });
                    
                    yield return new WaitUntil(() => _nav.SelectedResearch == questResearchType);
                    
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = true,
                            promptLocation = Vector2Int.up,
                            promptText = $"Select <color=green>Research</color>",
                            cutoutScreenSpace = GetScreenRect(CtaTag.HUDResearch)
                        }
                    });
                    break;
                case QuestType.Faith:
                    _showFtue.Dispatch(new[]
                    {
                        new FtuePrompt
                        {
                            blocking = false,
                            promptLocation = Vector2Int.left,
                            promptText = "Your <color=green>Faith</color>\ndepends on food and water stock pile,\nand boosts turns regen and cap",
                            cutoutScreenSpace = GetScreenRect(CtaTag.HUDFaith)
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Rect GetScreenRect(CtaTag ctaTag, int? payload = null)
        {
            //get rect transform from register
            var ctaTransform = _ctaRegister.Get(ctaTag, payload);
            if (ctaTransform is not RectTransform rectTransform)
            {
                Debug.LogError($"Failed to get rect transform for CtaTag: {ctaTag}[{payload}]");
                return Rect.zero;
            }

            var canvas = FindRenderingCanvas(rectTransform);

            // Get the world corners of the RectTransform
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Calculate the screen bounds
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            for (var i = 0; i < 4; i++)
            {
                // For Overlay canvas, world corners are already in screen space
                Vector2 screenPoint =
                    canvas.worldCamera == null
                        ? corners[i]
                        : canvas.worldCamera.WorldToScreenPoint(corners[i]);

                // Find min/max for creating our bounding rect
                min.x = Mathf.Min(min.x, screenPoint.x);
                min.y = Mathf.Min(min.y, screenPoint.y);
                max.x = Mathf.Max(max.x, screenPoint.x);
                max.y = Mathf.Max(max.y, screenPoint.y);
            }

            // Create and return the screen rect
            var width = max.x - min.x;
            var height = max.y - min.y;
            return new Rect(min.x + width / 2, min.y + height / 2, width, height);
        }

        public static Canvas FindRenderingCanvas(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return null;

            // Start with the current transform and work upward
            Transform current = rectTransform.transform;

            // Walk up the hierarchy until we find a root canvas or run out of parents
            while (current != null)
            {
                Canvas canvas = current.GetComponent<Canvas>();
                if (canvas != null && canvas.isRootCanvas)
                    return canvas;

                current = current.parent;
            }

            return null;
        }

        private void OnDestroy()
        {
            _startFtueSequence.Remove(OnStartFtue);
            _stopFtueSequence.Remove(OnStopFtue);
        }
    }
}