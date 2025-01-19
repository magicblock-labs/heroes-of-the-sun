import { ChatbotWrapper } from "./wrappers/chatbot.wrapper";


describe("Test suite for: creating a chatbot", () => {

    const bot = new ChatbotWrapper();


    it("creates a bot", async () => {
        await bot.initialize();
    });

});
