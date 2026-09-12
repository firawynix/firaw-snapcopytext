const demos = {
  regiao: {
    number: "01",
    title: "Ajuste antes de confirmar",
    description: "Arraste, mova e redimensione a área escolhida pelos oito pontos cianos. Confirme só quando o enquadramento estiver certo.",
    points: ["Seleção em tempo real", "Abrir por Enter ou copiar por Ctrl+C", "Sem o Firaw aparecer na imagem"],
    image: "assets/firaw-app.png",
    alt: "Tela do modo de captura por região"
  },
  janela: {
    number: "02",
    title: "Escolha o programa certo",
    description: "O Firaw lista as janelas abertas com nome e dimensão para você confirmar o alvo antes da captura.",
    points: ["Abrir no editor ou copiar direto", "Ctrl+C copia a janela escolhida", "Funciona mesmo com outra janela à frente"],
    image: "assets/firaw-picker.png",
    alt: "Lista do Firaw para escolher uma janela"
  },
  monitor: {
    number: "03",
    title: "Um monitor inteiro",
    description: "Veja as telas disponíveis, a resolução de cada uma e qual é a principal. Depois, escolha com um clique.",
    points: ["Abrir no editor ou copiar direto", "Ctrl+C copia o monitor escolhido", "Identificação da tela principal"],
    image: "assets/firaw-picker.png",
    alt: "Lista do Firaw para escolher um monitor"
  },
  atalhos: {
    number: "04",
    title: "Três atalhos no Print Screen",
    description: "Print Screen seleciona uma região, Alt + Print Screen captura um monitor e Ctrl + Print Screen captura uma janela. Cada combinação fica com o Firaw ou volta ao comportamento original do Windows.",
    points: ["Perfil pronto: Aplicar os 3 do Firaw", "Firaw ou Windows original, linha a linha", "Atalho personalizado extra e início com o Windows"],
    image: "assets/firaw-settings.png",
    alt: "Preferências de atalho e inicialização do Firaw"
  }
};

const tabs = document.querySelectorAll(".demo-tab");
const number = document.querySelector("#demo-number");
const title = document.querySelector("#demo-title");
const description = document.querySelector("#demo-description");
const points = document.querySelector("#demo-points");
const image = document.querySelector("#demo-image");

tabs.forEach((tab) => {
  tab.addEventListener("click", () => {
    const demo = demos[tab.dataset.demo];
    tabs.forEach((item) => {
      const selected = item === tab;
      item.classList.toggle("is-active", selected);
      item.setAttribute("aria-selected", String(selected));
    });
    number.textContent = demo.number;
    title.textContent = demo.title;
    description.textContent = demo.description;
    points.replaceChildren(...demo.points.map((point) => {
      const item = document.createElement("li");
      item.textContent = point;
      return item;
    }));
    image.src = demo.image;
    image.alt = demo.alt;
  });
});
