
import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  const origin = request.headers.get("origin");

  if (!origin || origin !== request.nextUrl.origin) {
    return NextResponse.json(
      { message: "許可されていないリクエストです。" },
      { status: 403 },
    );
  }

  const baseUrl = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!baseUrl) {
    return NextResponse.json(
      { message: "API接続先が設定されていません。" },
      { status: 503 },
    );
  }

  const cookie = request.headers.get("cookie");

  try {
    const upstream = await fetch(
      `${baseUrl.replace(/\/+$/, "")}/api/admin/auth/logout`,
      {
        method: "POST",
        headers: cookie ? { Cookie: cookie } : {},
        cache: "no-store",
      },
    );

    const response = new NextResponse(null, {
      status: upstream.status,
    });

    response.headers.set("Cache-Control", "no-store");

    const setCookie = upstream.headers.get("set-cookie");

    if (setCookie) {
      response.headers.set("Set-Cookie", setCookie);
    }

    return response;
  } catch {
    return NextResponse.json(
      { message: "APIサーバーに接続できません。" },
      { status: 502 },
    );
  }
}
