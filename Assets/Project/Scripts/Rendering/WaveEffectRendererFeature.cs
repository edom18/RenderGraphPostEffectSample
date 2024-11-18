using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 画面に波紋を表示するポストエフェクトパス
/// </summary>
public class WaveEffectRenderPass : ScriptableRenderPass
{
    private Material _material;

    public WaveEffectRenderPass(Material material)
    {
        _material = material;
    }

    /// <summary>
    /// パスの処理で利用するデータ
    /// </summary>
    private class PassData
    {
        public Material Material;
        public TextureHandle SourceTexture;
    }

    /// <summary>
    /// 実際に描画処理を行うメソッド
    /// </summary>
    /// <param name="renderGraph">RenderGraph のインスタンス</param>
    /// <param name="frameData">描画に必要なフレームのデータを格納するコンテナ</param>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // コンテナから必要なリソースを取得する
        
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        // 入力とするテクスチャ（エフェクト対象）を resourceData から取得
        // activeColorTexture はカメラが描画したメインのカラーバッファ
        // ※ 取得するテクスチャはすべて TextureHandle 型になる
        //    RenderGraph が扱う RenderTexture データ・タイプは従来の RHandle ではなく、
        //    TextureHandle 型になる
        //
        // TextureHandle の特徴
        // - クラス内部に実際のリソースを直接保持せず、リソース ID だけ保持している
        // - リソースの確保・解放などは RenderGraph がすべて管理している
        //     - 必要なリソースは描画の前に必ず「申請」する必要がある（アクセス方式など）
        //     - 申請されたリソースは描画段階に入ってから確保される。言い換えると、描画段階前にはリソースにアクセスすることができない
        //     - 申請されたリソースは描画処理に使われる分だけ確保される（無駄な確保を防げる）
        //     - 手動でリソース解放する必要がない
        
        TextureHandle sourceTextureHandle = resourceData.activeColorTexture;

        // 入力テクスチャの情報を元に、出力用のテクスチャの Descriptor を作成
        
        TextureDesc descriptor = renderGraph.GetTextureDesc(sourceTextureHandle);
        
        // テクスチャの名前
        
        descriptor.name = "WavePostEffectTexture";
        descriptor.clearBuffer = false;
        descriptor.msaaSamples = MSAASamples.None;
        
        // ポストエフェクトの場合は深度バッファは不要なので Bit を 0 に
        
        descriptor.depthBufferBits = 0;

        // 新規テクスチャリソースの「申請」
        // 申請のため、この時点ではまだリソースは確保されていない
        
        TextureHandle wavePostEffectTextureHandle = renderGraph.CreateTexture(descriptor);

        // 波紋エフェクトの RasterRenderPass を作成
        
        using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass("WavePostEffectRenderPass", out PassData passData))
        {
            passData.Material = _material;
            passData.SourceTexture = sourceTextureHandle;

            // builder を通して RenderGraphPass に対して各種設定を行う
            // なお、描画ターゲットや他使用されるテクスチャは必ずこの段階で設定する必要がある
            // いわゆるビルダーパターン
            
            // 描画ターゲットに出力用のテクスチャを設定
            
            builder.SetRenderAttachment(wavePostEffectTextureHandle, 0, AccessFlags.Write);
            
            // 入力テクスチャの使用を宣言する
            
            builder.UseTexture(sourceTextureHandle, AccessFlags.Read);

            // 実際の描画関数を設定する( static 関数が推奨されてるよう)
            
            builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
            {
                RasterCommandBuffer cmd = context.cmd;
                Material material = data.Material;
                TextureHandle source = data.SourceTexture;
                
                Blitter.BlitTexture(cmd, source, Vector2.one, material, 0);
            });
        }

        renderGraph.AddBlitPass(wavePostEffectTextureHandle, sourceTextureHandle, Vector2.one, Vector2.zero, passName: "BlitWavePostEffectTextureToCameraColor");
    }
}

public class WaveEffectRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Material _material;
    
    private WaveEffectRenderPass _pass;

    public override void Create()
    {
        _pass = new WaveEffectRenderPass(_material)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // ランタイムでマテリアルを生成したりした場合はここで破棄する
            // e.g) _pass.Cleanup();
        }
    }
}