# Jogo Survivors Japonês (nome provisório: JogoJapones)

Documento base do projeto. Serve de contexto para qualquer agente de código (Claude Code, Codex) e para o próprio desenvolvedor. Se for usar no Codex, copie este arquivo como `AGENTS.md`.

Fale com o desenvolvedor em **português**, de forma direta e em etapas pequenas.

---

## 1. Visão geral

Jogo de ação no estilo **survivors-like** (Vampire Survivors), com **temática japonesa**: youkais, samurais, shinobis, templos, florestas de bambu, efeitos de tinta sumi-e.

- O jogador controla só o movimento. Os ataques são automáticos.
- Hordas de inimigos vêm de todos os lados. Matar inimigos dá experiência, e cada nível sobe gera uma escolha de upgrade.
- As partidas são divididas em etapas com mini bosses e um boss final.
- **Plataforma**: **só navegador (WebGL)**, no PC e no celular (decisão de 22/11/2026). Sem app nativo de Android ou iOS por enquanto. Testes no PC e no navegador do celular.
- **Primeira versão grátis**, sem anúncios e sem compras. Monetização entra depois, mas a arquitetura já deve estar preparada para ela (ver seção 10).
- Toda a arte e os personagens devem ser **originais**. As referências servem de inspiração, nunca para copiar personagens, nomes ou assets.

## 2. Referências

| Jogo | O que aproveitar |
|---|---|
| **Pokébonk** (principal) | Estrutura de região, zonas e santuário; HUD; tela de seleção; modos e dificuldades; visual 2.5D (sprites em cenário 3D) |
| **Megabonk** | Ritmo rápido, upgrades dentro da partida, mundo 3D com verticalidade |
| **Warhammer Survivors** | Pixel art, horda densa, números de dano grandes, efeitos brilhantes sobre fundo escuro |
| **Vampire Survivors** | Loop base do gênero |

## 3. Estrutura da partida

### Hierarquia
- **Região**: é o mapa. Cada região tem um tema, um clima e tipos de inimigo próprios.
- **Zona**: cada região tem **3 zonas** jogadas em sequência.
- **Santuário**: a etapa final da região. Só o **boss da região**, sem inimigos comuns, sem loot e sem exploração.

Partida completa no modo Clássico: Zona 1 → Zona 2 → Zona 3 → Santuário.

### Zonas
- Cada zona tem **teto de 10 minutos** (contagem regressiva).
- O tempo é um **limite, não uma obrigação**: se o jogador ficar forte o bastante, pode chamar o guardião antes.
- Dentro da zona existem:
  - **Inimigos comuns**: chegam em ondas cada vez mais densas.
  - **Elites**: mini bosses que aparecem ao longo da zona, mais fortes e com recompensa melhor.
  - **Guardião**: o mini boss da zona. Derrotá-lo encerra a zona. Cada zona tem uma lista de guardiões possíveis e **um é sorteado por partida**.
- O jogador explora o mapa e pega itens, baús e pontos de interesse marcados no minimapa.
- **Timer zerado**: o guardião aparece automaticamente, sem punição extra (decisão provisória de 23/11/2026, configurável). O timer para enquanto o guardião está em campo.

### Exemplo de regiões (provisório)
Floresta de Bambu, Vila Costeira, Montanha Vulcânica, Templo Nevado, Cemitério de Youkais. Nomes, temas e inimigos serão definidos depois.

## 4. Combate e progressão dentro da partida

- **Ataque automático**: as habilidades disparam sozinhas, em intervalos.
- **Experiência**: inimigos derrubam experiência. Ao subir de nível, aparecem **cartas de upgrade** para escolher uma. Tipos de carta:
  - nova habilidade;
  - fortalecer habilidade que já tem;
  - passiva ou bônus de atributo.
- **Tetos**: existe teto de nível de cada habilidade e teto de quantidade de habilidades e passivas.
- **Espaços (layout inicial, ajustar no balanceamento)**:
  - 1 habilidade principal + 3 espaços de habilidades;
  - 3 espaços de passivas;
  - 1 habilidade ativa com recarga (tecla R no PC, botão na tela no celular);
  - 1 ultimate (tecla C no PC, botão na tela no celular).
- **Fusão por receitas**: duas habilidades no **nível máximo** podem se fundir numa habilidade nova e mais forte (ex.: fogo + pedra = habilidade de fogo e pedra). As receitas são **descobertas** pelo jogador e ficam registradas numa coleção.
- **Catálogo de habilidades, armas e itens**: ainda **não definido**. Usar habilidades provisórias simples no protótipo.

## 5. Fora da partida (meta-jogo)

Inspirado na tela de seleção do Pokébonk:

- **Seleção de região**: regiões são desbloqueadas aos poucos. Cada uma mostra tema, tipos de inimigo, descrição curta e os guardiões possíveis de cada zona e o boss do santuário.
- **Seleção de personagem**: escolher entre os personagens desbloqueados.
- **Modos**:
  - **Clássico**: zonas, guardiões e santuário.
  - **Sobrevivência**: hordas infinitas com marcos.
- **Dificuldades**: Normal, Difícil e Pesadelo, desbloqueadas aos poucos. Dificuldades maiores aumentam a força dos inimigos e multiplicam a moeda de progressão.
- **Metas**: recorde de abates, medalhas por tempo de conclusão (bronze, prata, ouro), recordes por região.
- **Mochila**: o jogador leva até 2 itens para a partida.
- **Coleções** (já no protótipo, cena `MainMenu`): **Manual de Criaturas** e **Habilidades**, mostrando o que o jogador já encontrou e o que falta (silhueta "???"). O registro fica no perfil do jogador.
- **Futuro**: temporadas, ranking global e por região, clãs, modo ranqueado.

## 6. HUD da partida

- **Topo esquerdo**: retrato do personagem, nome, nível da conta e nível dentro da partida.
- **Topo central**: nome da região, zona atual (ex.: 1/3), dificuldade, contagem regressiva da zona, tempo total, recorde, contadores de moedas e abates.
- **Topo direito**: minimapa redondo com pontos de interesse.
- **Centro**: barra de vida sob o personagem, com regeneração.
- **Embaixo**: espaços de habilidades e passivas, habilidade ativa e ultimate com indicador de recarga.
- **Celular**: joystick virtual e botões da ativa e do ultimate.
- **Números de dano**: grandes, em amarelo e branco, saindo dos inimigos atingidos.

## 7. Estilo visual

- **2.5D**: cenário em 3D com relevo (crateras, paredões, morros) e personagens, inimigos, plantas e efeitos como **sprites 2D em pixel art** sempre virados para a câmera (billboard).
- Texturas do terreno também em pixel art, com filtro **Point** (sem suavizar).
- Câmera de cima, inclinada, em perspectiva, seguindo o jogador.
- Paleta inspirada em ukiyo-e: vermelho de torii, dourado, preto, verdes e azuis. Efeitos de golpe com cara de tinta.
- Personagens pequenos na tela (sprites de 32 a 64 px), com silhueta fácil de reconhecer no meio da horda.
- **Protótipo**: arte provisória simples (formas e cores sólidas ou pixel art básico). A arte final vem depois.

## 8. Stack

### Jogo
- **Engine**: Unity 6.3 LTS (versão do projeto: 6000.3.29f1, instalada em D:/6000.3.29f1). Linguagem C#. Build target: WebGL.
- **Render**: URP.
- **Cenário**: Unity Terrain ou ProBuilder, com texturas em pixel art.
- **Câmera**: Cinemachine.
- **Controles**: Input System (novo), teclado no PC e joystick virtual no celular.
- **Interface**: UGUI + TextMeshPro. Minimapa com uma segunda câmera e Render Texture.
- **Horda**: pooling de objetos (`UnityEngine.Pool`) e sprites agrupados. Se o desempenho não der conta, migrar a parte crítica para Jobs + Burst. Não usar DOTS/ECS no começo.

### Arte e áudio
- Pixel art: Aseprite (ou LibreSprite).
- Conceitos: Higgsfield (geração de imagens).
- Áudio: sistema de áudio da Unity.

### Servidor (entra depois do protótipo)
- **Supabase**: login anônimo (convidado) com conversão para conta, Postgres para progresso, inventário, recordes, temporadas e clãs, e Edge Functions para validar recordes no servidor.

### Monetização (futuro)
- Como o jogo é só web, a monetização vai depender da plataforma de hospedagem (ex.: itch.io, portais de jogos web). Unity IAP e AdMob só entram se um dia houver app nativo.

### Ferramentas e publicação
- Código: Claude Code / Codex na pasta do projeto; editor VS Code ou Rider.
- Versionamento: Git + GitHub, com **Git LFS** para imagens, áudio e modelos.
- Builds: só WebGL, com compressão Gzip e fallback de descompressão (funciona em qualquer servidor), hospedado no itch.io.

## 9. Arquitetura e convenções

### Pastas
```
Assets/_Project/
  Scenes/
  Scripts/
    Core/          (estado do jogo, fluxo de partida, eventos)
    Player/
    Enemies/
    Combat/        (habilidades, projéteis, dano)
    Progression/   (XP, níveis, cartas, fusões)
    World/         (regiões, zonas, spawn, timer)
    UI/
    Utils/
  Sprites/
  Prefabs/
  Materials/
  ScriptableObjects/
  Audio/
```

### Regras de código
- Nomes de classes, métodos e variáveis em **inglês**. Comentários em **português**.
- Um `MonoBehaviour` por arquivo, com o mesmo nome do arquivo.
- **Todo dado de balanceamento em ScriptableObjects**, nunca fixo no código: regiões, zonas, inimigos, elites, guardiões, bosses, habilidades, passivas, receitas de fusão, curva de XP, dificuldades.
- Inimigos, projéteis, números de dano e itens no chão sempre com **pooling**. Nada de `Instantiate`/`Destroy` por frame.
- Comunicação entre sistemas por **eventos** (C# events ou ScriptableObject events), evitando referências diretas espalhadas.
- Evitar `Find` e `GetComponent` dentro de `Update`.
- Código simples e legível. O desenvolvedor programa com ajuda de IA, então prefira clareza a truques.

### Desempenho (alvo)
- Rodar a 60 fps no navegador de um celular intermediário com pelo menos **300 inimigos** na tela. WebGL é mais lento que app nativo, então pooling e sprites leves são obrigatórios desde o início.
- Medir com o Profiler da Unity antes de otimizar.

## 10. Preparação para monetização futura

Não implementar monetização agora, mas deixar a base pronta:

- **Conta do jogador**: mesmo como convidado, o progresso deve poder ser salvo num perfil ligado a um ID.
- **Inventário separado**: personagens, skins e desbloqueios ficam numa lista do jogador, não fixos no código. Liberar algo deve ser só acrescentar um item nessa lista.
- **Moeda do jogo**: uma moeda ganha jogando (nome provisório) desde o início.
- Possibilidades futuras: skins e personagens cosméticos, anúncio recompensado opcional (reviver, dobrar loot), passe de temporada cosmético, versão paga sem anúncios.

## 11. Roteiro do protótipo

Cada etapa deve terminar **jogável** e com **commit** no Git. Fazer uma etapa por vez e esperar o desenvolvedor testar.

0. **Ambiente**: projeto Unity URP criado, pacotes (Input System, TextMeshPro, Cinemachine), estrutura de pastas, `.gitignore` de Unity, Git + LFS, cena `Prototype` com chão, blocos de alturas diferentes, luz e câmera inclinada, e script `Billboard.cs`.
1. **Movimento**: personagem sprite andando com teclado e joystick virtual, câmera seguindo, colisão com o cenário.
2. **Horda**: inimigos nascendo ao redor fora da tela e perseguindo o jogador, com pooling e ondas que aumentam com o tempo.
3. **Combate**: uma habilidade automática, vida dos inimigos, morte, números de dano, vida e morte do jogador.
4. **Progressão**: experiência derrubada pelos inimigos, barra de XP, level-up com 3 cartas, 3 ou 4 habilidades provisórias e tetos de nível.
5. **Zona**: timer de 10 minutos, elites, guardião sorteado, opção de chamar o guardião antes, transição para a próxima zona.
6. **Região completa**: Zona 1 → 2 → 3 → Santuário com boss, tela de vitória e derrota.
7. **HUD completo**: layout da seção 6, incluindo minimapa.
7b. **Menu inicial** (pedido antes da etapa 8): cena `MainMenu` com Jogar, Manual de Criaturas e Habilidades.
8. **Fusão**: primeira receita funcionando (duas habilidades no máximo viram uma nova).
9. **Build de teste**: WebGL rodando no navegador do PC e do celular.

Depois do protótipo: tela de seleção (seção 5), Supabase, arte definitiva, catálogo de habilidades.

## 12. Fora do escopo agora

- Catálogo final de habilidades, armas, itens e receitas.
- Arte, música e efeitos definitivos.
- Servidor, contas, rankings, clãs e temporadas.
- Monetização.
- Builds nativos (Android e iOS).

## 13. Como o agente deve trabalhar

- Antes de começar uma etapa, explique em poucas linhas o que vai fazer.
- Faça mudanças pequenas, testáveis, e um commit por etapa.
- Não instale programas no sistema sem perguntar. Pacotes da Unity pelo `manifest.json` podem ser adicionados.
- Se algo só puder ser feito dentro do editor da Unity (arrastar referências, configurar cena), explique o passo a passo de forma simples ou, quando possível, faça por script de Editor.
- Ao terminar, diga: o que foi feito, como testar (o que apertar e o que deve acontecer) e o que ficou pendente.
- Mantenha este arquivo atualizado quando uma decisão de projeto mudar.
