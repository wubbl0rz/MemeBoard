// javascript OMEGALUL

new Vue({
  data: {
    mode: "dark-mode",
    modeIcon: "🌛",
    con: null,
    filter: "",
    memes: []
  },
  computed:{
    filteredMemes() {
      return this.memes.filter(m => {
        var checks = [
          m.name.toLowerCase(),
          m.type.toLowerCase(),
          m.prefix.toLowerCase()
        ];

        var pattern = this.filter.toLowerCase();

        return checks.some(m => m.includes(pattern))
      });
    }
  },
  methods:{
    toggleTheme(event) {
      if (this.mode == "dark-mode") {
        this.mode = "light-mode"; 
        this.modeIcon = "🌞";
      }
      else {
        this.mode = "dark-mode"; 
        this.modeIcon = "🌛";
      }
    },
    selected(meme) {
      console.log(meme);
      
      this.con.invoke("MemeClicked", meme.path);
    },
    updated(memes) {
      this.memes = memes.map(m => {
        if (m.type == "TTS")
          m.url = "audio.svg";
        else
          m.url = "/img/" + m.name;

        return m;
      });      
    }
  },
  created() {
    this.con = new signalR.HubConnectionBuilder().withUrl("/MemeHub").build();
  },
  mounted() {
    this.con.start();
    this.con.on("Update", this.updated);
  },
  el: '#app',
});