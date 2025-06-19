using UnityEngine;

namespace Survivor.Environment
{
    /// <summary>
    /// Handles underwater visual effects using Built-in Render Pipeline
    /// </summary>
    public class UnderwaterEffect : MonoBehaviour, IUnderwaterEffect
    {
        [Header("Underwater Visual Settings")]
        [SerializeField] private Color _underwaterTint = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        [SerializeField] private float _underwaterBlur = 0.5f;
        [SerializeField] private float _underwaterDistortion = 0.1f;
        [SerializeField] private float _transitionSpeed = 2f;
        [SerializeField] private float _effectIntensity = 1f;

        // Interface properties
        public Color underwaterTint { get => _underwaterTint; set => _underwaterTint = value; }
        public float underwaterBlur { get => _underwaterBlur; set => _underwaterBlur = value; }
        public float underwaterDistortion { get => _underwaterDistortion; set => _underwaterDistortion = value; }
        public float transitionSpeed { get => _transitionSpeed; set => _transitionSpeed = value; }
        public float effectIntensity { get => _effectIntensity; set => _effectIntensity = value; }

        [Header("Effect Components")]
        public Material underwaterMaterial;
        public Camera playerCamera;

        private float currentSubmersionLevel = 0f;
        private float targetSubmersionLevel = 0f;
        private bool effectsInitialized = false;
        private Material overlayMaterial;
        private Vector3 originalCameraPosition;

        private void Start()
        {
            InitializeEffects();
        }

        private void InitializeEffects()
        {
            // Find player camera
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    playerCamera = FindObjectOfType<Camera>();
                }
            }

            if (playerCamera == null)
            {
                Debug.LogWarning("[UnderwaterEffect] No camera found! Underwater effects will not work.");
                return;
            }

            // Store original camera position
            originalCameraPosition = playerCamera.transform.localPosition;

            // Create underwater material if not assigned
            if (underwaterMaterial == null)
            {
                CreateUnderwaterMaterial();
            }

            // Create overlay material
            CreateOverlayMaterial();

            effectsInitialized = true;
        }

        private void CreateUnderwaterMaterial()
        {
            // Create a simple underwater material for screen overlay
            underwaterMaterial = new Material(Shader.Find("Standard"));
            underwaterMaterial.color = underwaterTint;
            underwaterMaterial.SetFloat("_Glossiness", 0.9f);
            underwaterMaterial.SetFloat("_Metallic", 0.0f);
        }

        private void CreateOverlayMaterial()
        {
            // Create a simple overlay shader for Built-in RP
            string shaderCode = @"
                Shader ""Custom/UnderwaterOverlay""
                {
                    Properties
                    {
                        _MainTex (""Texture"", 2D) = ""white"" {}
                        _TintColor (""Tint Color"", Color) = (0.2, 0.4, 0.8, 0.3)
                        _Intensity (""Intensity"", Range(0, 1)) = 0.5
                    }
                    SubShader
                    {
                        Tags { ""Queue""=""Transparent"" ""RenderType""=""Transparent"" }
                        Blend SrcAlpha OneMinusSrcAlpha
                        ZWrite Off
                        Cull Off

                        Pass
                        {
                            CGPROGRAM
                            #pragma vertex vert
                            #pragma fragment frag
                            #include ""UnityCG.cginc""

                            struct appdata
                            {
                                float4 vertex : POSITION;
                                float2 uv : TEXCOORD0;
                            };

                            struct v2f
                            {
                                float2 uv : TEXCOORD0;
                                float4 vertex : SV_POSITION;
                            };

                            sampler2D _MainTex;
                            float4 _TintColor;
                            float _Intensity;

                            v2f vert (appdata v)
                            {
                                v2f o;
                                o.vertex = UnityObjectToClipPos(v.vertex);
                                o.uv = v.uv;
                                return o;
                            }

                            fixed4 frag (v2f i) : SV_Target
                            {
                                fixed4 col = tex2D(_MainTex, i.uv);
                                col = lerp(col, _TintColor, _Intensity);
                                return col;
                            }
                            ENDCG
                        }
                    }
                }";

            overlayMaterial = new Material(shaderCode);
        }

        public void SetSubmersionLevel(float submersionLevel)
        {
            targetSubmersionLevel = submersionLevel;
        }

        private void Update()
        {
            if (!effectsInitialized) return;

            // Smoothly transition to target submersion level
            currentSubmersionLevel = Mathf.Lerp(currentSubmersionLevel, targetSubmersionLevel, 
                transitionSpeed * Time.deltaTime);

            // Apply effects based on submersion level
            ApplyUnderwaterEffects(currentSubmersionLevel * effectIntensity);
        }

        private void ApplyUnderwaterEffects(float intensity)
        {
            if (playerCamera == null) return;

            // Apply camera effects
            ApplyCameraEffects(intensity);

            // Apply screen overlay
            if (overlayMaterial != null && intensity > 0.01f)
            {
                ApplyScreenOverlay(intensity);
            }
        }

        private void ApplyCameraEffects(float intensity)
        {
            // Apply subtle camera shake for underwater effect
            if (intensity > 0.1f)
            {
                float shakeAmount = underwaterDistortion * intensity * 0.1f;
                Vector3 shake = new Vector3(
                    Mathf.Sin(Time.time * 2f) * shakeAmount,
                    Mathf.Cos(Time.time * 1.5f) * shakeAmount,
                    0f
                );
                playerCamera.transform.localPosition = originalCameraPosition + shake;
            }
            else
            {
                playerCamera.transform.localPosition = originalCameraPosition;
            }

            // Apply color grading effect
            if (intensity > 0.01f)
            {
                // Modify camera's clear color slightly
                Color clearColor = playerCamera.backgroundColor;
                clearColor = Color.Lerp(clearColor, underwaterTint, intensity * 0.3f);
                playerCamera.backgroundColor = clearColor;
            }
        }

        private void ApplyScreenOverlay(float intensity)
        {
            if (overlayMaterial != null)
            {
                // Set material properties
                overlayMaterial.SetColor("_TintColor", underwaterTint);
                overlayMaterial.SetFloat("_Intensity", intensity * 0.5f);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (overlayMaterial != null && currentSubmersionLevel > 0.01f)
            {
                Graphics.Blit(source, destination, overlayMaterial);
            }
            else
            {
                Graphics.Blit(source, destination);
            }
        }

        private void OnDestroy()
        {
            // Clean up created materials
            if (underwaterMaterial != null)
            {
                DestroyImmediate(underwaterMaterial);
            }
            if (overlayMaterial != null)
            {
                DestroyImmediate(overlayMaterial);
            }
        }

        // Public methods for runtime configuration
        public void SetUnderwaterTint(Color tint)
        {
            underwaterTint = tint;
        }

        public void SetUnderwaterBlur(float blur)
        {
            underwaterBlur = blur;
        }

        public void SetUnderwaterDistortion(float distortion)
        {
            underwaterDistortion = distortion;
        }

        public void SetTransitionSpeed(float speed)
        {
            transitionSpeed = speed;
        }

        public void SetEffectIntensity(float intensity)
        {
            effectIntensity = intensity;
        }
    }
} 