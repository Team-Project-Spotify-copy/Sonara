
export const DEV_CAPTCHA_TOKEN = "dev-dummy-token";

export async function resolveCaptchaToken(executeRecaptcha, action) {
  let token = null;

  if (executeRecaptcha) {
    try {
      token = await executeRecaptcha(action);
    } catch {
      token = null;
    }
  }

  if (token) return token;

  return import.meta.env.DEV ? DEV_CAPTCHA_TOKEN : null;
}
