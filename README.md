# AIR IA Code

Assistente nativo para Windows que executa modelos de inteligência artificial localmente, trabalha em projetos de programação e gera imagens e vídeos sem depender de serviços externos.

Desenvolvido por **Codename Jackers**.

## Recursos

- Chat local com modelos GGUF e respostas em streaming.
- Catálogo de modelos com download retomável e modelo padrão.
- Projetos com conversas individuais, acesso a arquivos, terminal, build e testes.
- Agente autônomo persistente: até 8 horas e 200 ciclos, checkpoints, retomada e validação automática.
- Mapeamento de projetos, leitura em lote e criação, edição, cópia, movimentação e exclusão controlada de arquivos.
- Ferramentas de Git, ADB e Logcat exibidas em tempo real no chat.
- Estúdio local de imagens e vídeos com configuração automática.
- Suporte a AMD DirectML, NVIDIA CUDA, Intel XPU e CPU.
- Aplicativo WPF nativo e instalador para Windows.

## Compilar

Requisitos: Windows 10/11 e .NET SDK 9.

```powershell
dotnet build AirCodeNative.csproj -c Release
```

Para gerar o instalador, abra `installer.iss` no Inno Setup 6 ou execute:

```powershell
ISCC.exe installer.iss
```

## Dados locais

Modelos, conversas e configurações são mantidos fora do repositório, em `%LOCALAPPDATA%\AirCodeLocal`.

## Desenvolvedor

Codename **Jackers**
