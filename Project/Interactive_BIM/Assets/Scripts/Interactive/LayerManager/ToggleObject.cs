using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObject : MonoBehaviour
{
    public enum ToggleMode
    {
        Show,
        Fade,
        Hide
    }

    public struct ToggleableMaterial
    {
        public Material OriginalMaterial;
        public Material FadeMaterial;

        public ToggleableMaterial(Material original, Material fade)
        {
            OriginalMaterial = original;
            FadeMaterial = fade;
        }

        public void ChangeMaterial(MeshRenderer mr, ToggleMode mode)
        {
            switch (mode)
            {
                case ToggleMode.Show:
                    mr.material = OriginalMaterial;
                    mr.gameObject.SetActive(true);
                    break;
                case ToggleMode.Fade:
                    mr.material = FadeMaterial;
                    mr.gameObject.SetActive(true);
                    break;
                case ToggleMode.Hide:
                    mr.gameObject.SetActive(false);
                    break;
            }
        }
    }

    private static Dictionary<MeshRenderer, ToggleableMaterial> _meshes = new Dictionary<MeshRenderer, ToggleableMaterial>();

    public void Toggle(MeshRenderer mr, ToggleMode mode)
    {
        if (!_meshes.ContainsKey(mr))
            _meshes.Add(mr, CreateMaterials(mr));

        _meshes[mr].ChangeMaterial(mr, mode);
    }

    private ToggleableMaterial CreateMaterials(MeshRenderer mr)
    {
        var originalMaterial = Instantiate(mr.material);

        var seethroughMaterial = Instantiate(mr.material);
        seethroughMaterial.name = mr.material.name + "_Seethrough";
        seethroughMaterial.SetFloat("_Mode", 2);
        seethroughMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        seethroughMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        seethroughMaterial.SetInt("_ZWrite", 0);
        seethroughMaterial.DisableKeyword("_ALPHATEST_ON");
        seethroughMaterial.EnableKeyword("_ALPHABLEND_ON");
        seethroughMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        seethroughMaterial.renderQueue = 3000;
        seethroughMaterial.color = new Color(seethroughMaterial.color.r, seethroughMaterial.color.g,
            seethroughMaterial.color.b, 0.2f);

        return new ToggleableMaterial(originalMaterial, seethroughMaterial);
    }
}